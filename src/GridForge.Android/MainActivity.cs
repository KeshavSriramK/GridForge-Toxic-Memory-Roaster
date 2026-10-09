using System;
using System.Collections.Generic;
using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using GridForge.Core.AI.Pathfinding;

namespace GridForge.Android;

public enum GameMode { None, Easy, Normal, Hard }
public enum GameState { NameInput, MainMenu, Tutorial, Memorizing, Playing, Victory, GameOver }

[Activity(
    Label = "GridForge",
    Icon = "@mipmap/icon",
    RoundIcon = "@mipmap/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public class MainActivity : Activity
{
    private MemoryRoasterView? _gameView;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        _gameView = new MemoryRoasterView(this);
        SetContentView(_gameView);
    }

    protected override void OnResume()
    {
        base.OnResume();
        _gameView?.Start();
    }

    protected override void OnPause()
    {
        base.OnPause();
        _gameView?.Stop();
    }
}

public class MemoryRoasterView : SurfaceView, ISurfaceHolderCallback
{
    private const int VirtualWidth = 800;
    private const int VirtualHeight = 700;
    private const int GridSize = 20;
    private const int CellSize = 25;
    private const int OffsetX = 150;
    private const int OffsetY = 40;

    private RoasterThread? _thread;
    private GameState _currentState = GameState.NameInput;
    private GameMode _currentMode = GameMode.None;

    private string? _userName = null;
    private string _nameInputBuffer = "";
    private const int MaxNameLength = 16;

    private int _currentLevel = 1;
    private int _currentGame = 1;
    private int _maxLevels = 5;
    private const int GamesPerLevel = 5;

    private float _timeRemaining = 0f;
    private float _maxTimeForLevel = 30f;

    private PathNode[,] _grid = null!;
    private AStarPathfinder _pathfinder = null!;
    private List<PathNode> _targetSolutionPath = new();
    private HashSet<(int X, int Y)> _targetSolutionSet = new();
    private HashSet<(int X, int Y)> _userDrawnPath = new();

    private string _statusMessage = "";
    private float _statusMessageTimer = 0f;

    private string _currentVictoryHeadline = "";
    private string _currentVictorySubtext = "";
    private string _currentVictoryBtnText = "";
    private string _currentGameOverHeadline = "";
    private string _currentGameOverSubtext = "";

    private readonly Random _random = new();

    // UI Buttons
    private RectF _btnEasy = new(150, 260, 650, 325);
    private RectF _btnNormal = new(150, 345, 650, 410);
    private RectF _btnHard = new(150, 430, 650, 495);
    private RectF _confirmNameBtn = new(250, 400, 550, 450);
    private RectF _startTutorialBtn = new(200, 580, 600, 635);
    private RectF _startPlayingButton = new(200, 625, 600, 670);
    private RectF _checkButton = new(140, 620, 290, 660);
    private RectF _resetButton = new(325, 620, 475, 660);
    private RectF _menuButton = new(510, 620, 660, 660);
    private RectF _nextGameButton = new(150, 400, 650, 450);
    private RectF _victoryMenuButton = new(150, 470, 650, 520);

    public MemoryRoasterView(Activity context) : base(context)
    {
        Holder?.AddCallback(this);
        Focusable = true;
    }

    public void SurfaceCreated(ISurfaceHolder holder)
    {
        _thread = new RoasterThread(Holder, this);
        _thread.Running = true;
        _thread.Start();
    }

    public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height) { }
    public void SurfaceDestroyed(ISurfaceHolder holder) => Stop();

    public void Start()
    {
        if (_thread == null)
        {
            _thread = new RoasterThread(Holder, this);
            _thread.Running = true;
            _thread.Start();
        }
    }

    public void Stop()
    {
        if (_thread != null)
        {
            _thread.Running = false;
            while (true)
            {
                try { _thread.Join(); break; } catch { }
            }
            _thread = null;
        }
    }

    private PointF GetVirtualInputPosition(float rawX, float rawY)
    {
        float scale = Math.Min((float)Width / VirtualWidth, (float)Height / VirtualHeight);
        float offX = (Width - (VirtualWidth * scale)) * 0.5f;
        float offY = (Height - (VirtualHeight * scale)) * 0.5f;

        float vx = Math.Clamp((rawX - offX) / scale, 0, VirtualWidth);
        float vy = Math.Clamp((rawY - offY) / scale, 0, VirtualHeight);
        return new PointF(vx, vy);
    }

    private bool IsHovered(PointF pos, RectF rect) => rect.Contains(pos.X, pos.Y);

    private void StartMode(GameMode mode)
    {
        _currentMode = mode;
        _currentLevel = 1;
        _currentGame = 1;

        _maxLevels = mode switch
        {
            GameMode.Easy => 5,
            GameMode.Normal => 10,
            _ => 20
        };

        _maxTimeForLevel = mode switch
        {
            GameMode.Easy => 30f,
            GameMode.Normal => 20f,
            _ => 15f
        };

        _currentState = GameState.Tutorial;
    }

    private void LoadGameStage(int level, int game)
    {
        _currentLevel = level;
        _currentGame = game;
        _timeRemaining = _maxTimeForLevel;
        _statusMessage = "";
        _targetSolutionPath.Clear();
        _targetSolutionSet.Clear();
        _userDrawnPath.Clear();

        _grid = new PathNode[GridSize, GridSize];
        for (int x = 0; x < GridSize; x++)
            for (int y = 0; y < GridSize; y++)
                _grid[x, y] = new PathNode(x, y, true);

        int globalGameIndex = ((level - 1) * GamesPerLevel) + game;
        int seed = ((int)_currentMode * 10000) + (level * 100) + game;
        Random rand = new(seed);

        double obstacleChance = _currentMode switch
        {
            GameMode.Easy => 0.08 + (globalGameIndex * 0.005),
            GameMode.Normal => 0.14 + (globalGameIndex * 0.004),
            _ => 0.22 + (globalGameIndex * 0.002)
        };

        for (int x = 3; x < 17; x++)
            for (int y = 2; y < 18; y++)
                if (!(x == 2 && y == 10) && !(x == 18 && y == 10) && rand.NextDouble() < obstacleChance)
                    _grid[x, y].IsWalkable = false;

        _pathfinder = new AStarPathfinder(_grid);
        _targetSolutionPath = _pathfinder.FindPath(2, 10, 18, 10);

        while (_targetSolutionPath == null || _targetSolutionPath.Count == 0)
        {
            for (int x = 0; x < GridSize; x++)
                for (int y = 0; y < GridSize; y++)
                    _grid[x, y].IsWalkable = true;

            for (int x = 3; x < 17; x++)
                for (int y = 2; y < 18; y++)
                    if (!(x == 2 && y == 10) && !(x == 18 && y == 10) && rand.NextDouble() < (obstacleChance * 0.7))
                        _grid[x, y].IsWalkable = false;

            _pathfinder = new AStarPathfinder(_grid);
            _targetSolutionPath = _pathfinder.FindPath(2, 10, 18, 10);
        }

        foreach (var node in _targetSolutionPath)
        {
            if ((node.X == 2 && node.Y == 10) || (node.X == 18 && node.Y == 10)) continue;
            _targetSolutionSet.Add((node.X, node.Y));
        }

        _currentState = GameState.Memorizing;
    }

    private bool ValidateExactUserPath()
    {
        if (_userDrawnPath.Count != _targetSolutionSet.Count) return false;
        foreach (var cell in _userDrawnPath)
            if (!_targetSolutionSet.Contains(cell)) return false;
        return true;
    }

    private string GetActivePlayerName() => string.IsNullOrWhiteSpace(_userName) ? "Genius" : _userName;

    private void TriggerVictory()
    {
        string name = GetActivePlayerName().ToUpper();
        string[] headlines = { $"OH, YOU ACTUALLY DID IT, {name}?", $"PURE BLIND LUCK, {name}!", $"WHO LET {name} WIN?" };
        string[] subtexts = { $"{name}, you managed to trace a line without drooling. Truly inspiring.", $"Don't let it go to your head {name}." };
        string[] btnTexts = { "Subject Yourself to More", "Prove It Wasn't A Fluke", "Next Level of Torture" };

        _currentVictoryHeadline = headlines[_random.Next(headlines.Length)];
        _currentVictorySubtext = subtexts[_random.Next(subtexts.Length)];
        _currentVictoryBtnText = btnTexts[_random.Next(btnTexts.Length)];
        _currentState = GameState.Victory;
    }

    private void TriggerGameOver()
    {
        string name = GetActivePlayerName().ToUpper();
        string[] headlines = { $"TIME'S UP, {name}.", $"{name} HAS THE MEMORY OF A GOLDFISH.", $"ABSOLUTE DISASTER, {name}!" };
        string[] subtexts = { $"That path was on screen for seconds. Did {name} forget how to see?", $"That was painful to watch, {name}." };

        _currentGameOverHeadline = headlines[_random.Next(headlines.Length)];
        _currentGameOverSubtext = subtexts[_random.Next(subtexts.Length)];
        _currentState = GameState.GameOver;
    }

    public override bool OnTouchEvent(MotionEvent? e)
    {
        if (e == null) return true;
        PointF pos = GetVirtualInputPosition(e.GetX(), e.GetY());

        if (e.Action == MotionEventActions.Down)
        {
            if (_currentState == GameState.NameInput)
            {
                _userName = "Player";
                _currentState = GameState.MainMenu;
            }
            else if (_currentState == GameState.MainMenu)
            {
                if (IsHovered(pos, _btnEasy)) StartMode(GameMode.Easy);
                else if (IsHovered(pos, _btnNormal)) StartMode(GameMode.Normal);
                else if (IsHovered(pos, _btnHard)) StartMode(GameMode.Hard);
            }
            else if (_currentState == GameState.Tutorial)
            {
                if (IsHovered(pos, _startTutorialBtn)) LoadGameStage(_currentLevel, _currentGame);
            }
            else if (_currentState == GameState.Memorizing)
            {
                if (IsHovered(pos, _startPlayingButton)) _currentState = GameState.Playing;
            }
            else if (_currentState == GameState.Playing)
            {
                if (IsHovered(pos, _checkButton))
                {
                    if (ValidateExactUserPath()) TriggerVictory();
                    else
                    {
                        string name = GetActivePlayerName();
                        string[] insults = { $"Are your eyes painted on, {name}?", $"Not even close, {name}.", $"Epic fail, {name}." };
                        _statusMessage = insults[_random.Next(insults.Length)];
                        _statusMessageTimer = 3.0f;
                    }
                }
                else if (IsHovered(pos, _resetButton)) _userDrawnPath.Clear();
                else if (IsHovered(pos, _menuButton)) _currentState = GameState.MainMenu;
            }
            else if (_currentState == GameState.Victory || _currentState == GameState.GameOver)
            {
                if (IsHovered(pos, _nextGameButton))
                {
                    int nextGame = _currentGame + 1;
                    int nextLevel = _currentLevel;
                    if (nextGame > GamesPerLevel) { nextGame = 1; nextLevel++; if (nextLevel > _maxLevels) nextLevel = 1; }
                    LoadGameStage(nextLevel, nextGame);
                }
                else if (IsHovered(pos, _victoryMenuButton)) _currentState = GameState.MainMenu;
            }
        }

        if (_currentState == GameState.Playing && (e.Action == MotionEventActions.Down || e.Action == MotionEventActions.Move))
        {
            int gx = (int)((pos.X - OffsetX) / CellSize);
            int gy = (int)((pos.Y - OffsetY) / CellSize);
            if (gx >= 0 && gx < GridSize && gy >= 0 && gy < GridSize)
            {
                if (_grid[gx, gy].IsWalkable && !((gx == 2 && gy == 10) || (gx == 18 && gy == 10)))
                    _userDrawnPath.Add((gx, gy));
            }
        }

        return true;
    }

    public void Update(float deltaTime)
    {
        if (_statusMessageTimer > 0)
        {
            _statusMessageTimer -= deltaTime;
            if (_statusMessageTimer <= 0) _statusMessage = "";
        }

        if (_currentState == GameState.Playing)
        {
            _timeRemaining -= deltaTime;
            if (_timeRemaining <= 0)
            {
                _timeRemaining = 0;
                TriggerGameOver();
            }
        }
    }

    public new void OnDraw(Canvas canvas)
    {
        base.OnDraw(canvas);

        float scale = Math.Min((float)Width / VirtualWidth, (float)Height / VirtualHeight);
        float offX = (Width - (VirtualWidth * scale)) * 0.5f;
        float offY = (Height - (VirtualHeight * scale)) * 0.5f;

        canvas.DrawColor(Color.Black);
        canvas.Save();
        canvas.Translate(offX, offY);
        canvas.Scale(scale, scale);

        // Virtual Canvas Background
        using var paint = new Paint { AntiAlias = true };
        paint.Color = Color.ParseColor("#0F172A");
        canvas.DrawRect(0, 0, VirtualWidth, VirtualHeight, paint);

        if (_currentState == GameState.NameInput)
        {
            paint.Color = Color.ParseColor("#38BDF8");
            paint.TextSize = 28;
            paint.TextAlign = Paint.Align.Center;
            canvas.DrawText("WHO IS DARING TO PLAY?", VirtualWidth / 2f, 160, paint);

            paint.Color = Color.ParseColor("#22C55E");
            canvas.DrawRect(_confirmNameBtn, paint);

            paint.Color = Color.Black;
            paint.TextSize = 20;
            canvas.DrawText("TAP TO START SESSION", VirtualWidth / 2f, 432, paint);
        }
        else if (_currentState == GameState.MainMenu)
        {
            paint.Color = Color.ParseColor("#38BDF8");
            paint.TextSize = 34;
            paint.TextAlign = Paint.Align.Center;
            canvas.DrawText("MEMORY ROASTER 3000", VirtualWidth / 2f, 90, paint);

            paint.Color = Color.ParseColor("#16A34A"); canvas.DrawRect(_btnEasy, paint);
            paint.Color = Color.ParseColor("#EAB308"); canvas.DrawRect(_btnNormal, paint);
            paint.Color = Color.ParseColor("#E11D48"); canvas.DrawRect(_btnHard, paint);

            paint.Color = Color.White;
            paint.TextSize = 20;
            canvas.DrawText("EASY (For people with 3 brain cells)", VirtualWidth / 2f, 298, paint);
            canvas.DrawText("NORMAL (You'll probably still fail)", VirtualWidth / 2f, 383, paint);
            canvas.DrawText("HARD (Prepare to cry)", VirtualWidth / 2f, 468, paint);
        }
        else if (_currentState == GameState.Tutorial)
        {
            paint.Color = Color.ParseColor("#38BDF8");
            paint.TextSize = 26;
            paint.TextAlign = Paint.Align.Left;
            canvas.DrawText($"WELCOME, {GetActivePlayerName().ToUpper()}!", 100, 80, paint);

            paint.Color = Color.White;
            paint.TextSize = 18;
            canvas.DrawText("1. Look at the blue path. Try to memorize it.", 100, 150, paint);
            canvas.DrawText("2. Tap START to hide it.", 100, 200, paint);
            canvas.DrawText("3. Drag your finger across grid cells to redraw the path.", 100, 250, paint);
            canvas.DrawText("4. Tap CHECK WORK to validate your path.", 100, 300, paint);

            paint.Color = Color.ParseColor("#15803D"); canvas.DrawRect(_startTutorialBtn, paint);
            paint.Color = Color.White; paint.TextAlign = Paint.Align.Center;
            canvas.DrawText("I DARE TO TRY", VirtualWidth / 2f, 615, paint);
        }
        else if (_currentState == GameState.Memorizing || _currentState == GameState.Playing)
        {
            // HUD
            paint.Color = Color.White; paint.TextSize = 18; paint.TextAlign = Paint.Align.Left;
            canvas.DrawText($"Player: {GetActivePlayerName()} | {_currentMode} Lvl {_currentLevel}/{_maxLevels}", 160, 25, paint);

            if (_currentState == GameState.Playing)
            {
                float timerPct = _timeRemaining / _maxTimeForLevel;
                paint.Color = timerPct > 0.4f ? Color.ParseColor("#15803D") : Color.ParseColor("#B91C1C");
                canvas.DrawRect(150, 32, 150 + (500 * timerPct), 38, paint);
            }

            // Grid Rendering
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    var node = _grid[x, y];
                    paint.Color = node.IsWalkable ? Color.LTGRAY : Color.DKGRAY;

                    if (_currentState == GameState.Memorizing && _targetSolutionPath.Contains(node))
                        paint.Color = Color.ParseColor("#38BDF8");
                    else if (_currentState == GameState.Playing && _userDrawnPath.Contains((x, y)))
                        paint.Color = Color.ParseColor("#38BDF8");

                    canvas.DrawRect(OffsetX + x * CellSize, OffsetY + y * CellSize,
                                    OffsetX + (x + 1) * CellSize - 2, OffsetY + (y + 1) * CellSize - 2, paint);
                }
            }

            // Start (Green) and End (Red) Nodes
            paint.Color = Color.GREEN;
            canvas.DrawRect(OffsetX + 2 * CellSize, OffsetY + 10 * CellSize, OffsetX + 3 * CellSize - 2, OffsetY + 11 * CellSize - 2, paint);
            paint.Color = Color.RED;
            canvas.DrawRect(OffsetX + 18 * CellSize, OffsetY + 10 * CellSize, OffsetX + 19 * CellSize - 2, OffsetY + 11 * CellSize - 2, paint);

            if (_currentState == GameState.Memorizing)
            {
                paint.Color = Color.ParseColor("#15803D"); canvas.DrawRect(_startPlayingButton, paint);
                paint.Color = Color.White; paint.TextAlign = Paint.Align.Center; paint.TextSize = 18;
                canvas.DrawText("I'M READY (START)", VirtualWidth / 2f, 652, paint);
            }
            else
            {
                paint.Color = Color.ParseColor("#15803D"); canvas.DrawRect(_checkButton, paint);
                paint.Color = Color.ParseColor("#1E3A8A"); canvas.DrawRect(_resetButton, paint);
                paint.Color = Color.ParseColor("#881337"); canvas.DrawRect(_menuButton, paint);

                paint.Color = Color.White; paint.TextSize = 14; paint.TextAlign = Paint.Align.Center;
                canvas.DrawText("CHECK WORK", _checkButton.CenterX(), 644, paint);
                canvas.DrawText("Wipe Shame", _resetButton.CenterX(), 644, paint);
                canvas.DrawText("Give Up", _menuButton.CenterX(), 644, paint);

                if (!string.IsNullOrEmpty(_statusMessage))
                {
                    paint.Color = Color.ParseColor("#F85149"); paint.TextSize = 16;
                    canvas.DrawText(_statusMessage, VirtualWidth / 2f, 595, paint);
                }
            }
        }
        else if (_currentState == GameState.Victory || _currentState == GameState.GameOver)
        {
            paint.Color = _currentState == GameState.Victory ? Color.ParseColor("#15803D") : Color.ParseColor("#B91C1C");
            paint.TextSize = 22; paint.TextAlign = Paint.Align.Center;
            string title = _currentState == GameState.Victory ? _currentVictoryHeadline : _currentGameOverHeadline;
            canvas.DrawText(title, VirtualWidth / 2f, 180, paint);

            paint.Color = Color.White; paint.TextSize = 16;
            string sub = _currentState == GameState.Victory ? _currentVictorySubtext : _currentGameOverSubtext;
            canvas.DrawText(sub, VirtualWidth / 2f, 240, paint);

            paint.Color = Color.ParseColor("#1E3A8A"); canvas.DrawRect(_nextGameButton, paint);
            paint.Color = Color.ParseColor("#881337"); canvas.DrawRect(_victoryMenuButton, paint);

            paint.Color = Color.White; paint.TextSize = 18;
            string btn1 = _currentState == GameState.Victory ? _currentVictoryBtnText : "Embarrass Yourself Again";
            canvas.DrawText(btn1, VirtualWidth / 2f, 430, paint);
            canvas.DrawText("Flee to Main Menu", VirtualWidth / 2f, 500, paint);
        }

        canvas.Restore();
    }
}

public class RoasterThread : Thread
{
    private readonly ISurfaceHolder _holder;
    private readonly MemoryRoasterView _view;
    public bool Running { get; set; }

    public RoasterThread(ISurfaceHolder holder, MemoryRoasterView view)
    {
        _holder = holder;
        _view = view;
    }

    public override void Run()
    {
        long lastTime = SystemClock.ElapsedRealtime();

        while (Running)
        {
            Canvas? canvas = null;
            long now = SystemClock.ElapsedRealtime();
            float deltaTime = (now - lastTime) / 1000f;
            lastTime = now;

            try
            {
                canvas = _holder.LockCanvas();
                if (canvas != null)
                {
                    lock (_holder)
                    {
                        _view.Update(deltaTime);
                        _view.OnDraw(canvas);
                    }
                }
            }
            finally
            {
                if (canvas != null) _holder.UnlockCanvasAndPost(canvas);
            }
        }
    }
}