using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using GridForge.Core.AI.Pathfinding;

namespace GridForge.Android;

public enum GameMode { None, Easy, Normal, Hard }
public enum GameState { NameInput, MainMenu, ModeVerdict, Tutorial, Memorizing, Playing, Victory, GameOver }

public class MonoGameApp : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private readonly MainActivity _activity;

    private const int TargetWidth = 800;
    private const int TargetHeight = 700;
    private const int GridSize = 20;
    private const int CellSize = 25;

    private const int GridOffsetX = 150;
    private const int GridOffsetY = 100;

    private float _scale = 1f;
    private Vector2 _screenOffset = Vector2.Zero;
    private float _layoutTimer = 1f; // forces a layout pass on the first frame

    private GameState _currentState = GameState.NameInput;
    private GameMode _currentMode = GameMode.None;

    private string _userName = "";
    private int _currentLevel = 1;
    private int _currentGame = 1;
    private int _maxLevels = 5;
    private const int GamesPerLevel = 5;

    private float _timeRemaining = 30f;
    private float _maxTimeForLevel = 30f;

    private float _peekTimer = 0f;

    private PathNode[,] _grid = null!;
    private AStarPathfinder _pathfinder = null!;
    private List<PathNode> _targetSolutionPath = new();
    private HashSet<(int X, int Y)> _targetSolutionSet = new();
    private HashSet<(int X, int Y)> _userDrawnPathSet = new();

    private string _statusMessage = "";
    private float _statusMessageTimer = 0f;

    private string _currentVictoryTitle = "";
    private string _currentVictorySubtext = "";
    private string _currentGameOverTitle = "";
    private string _currentGameOverSubtext = "";
    private string _currentPreGameRoast = "";

    // ---- per-round stats (used by the result screen) ----
    private int _wrongChecks, _peeksUsed, _cellsWiped;
    private float _resultTime;
    private int _shame;
    private float _timeUsed;
    private string _achievement = "";
    private string[] _resultInsults = Array.Empty<string>();

    // ---- difficulty verdict screen ----
    private string _verdictTitle = "";
    private string _verdictText = "";
    private string[] _verdictStats = Array.Empty<string>();

    private readonly Random _random = new();

    private struct BgParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;
        public Color Color;
    }
    private readonly List<BgParticle> _particles = new();

    private struct Confetti
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public char Glyph;
        public int Scale;
        public Color Color;
    }
    private readonly List<Confetti> _confetti = new();

    // Name screen: the yellow box is where the native EditText gets overlaid.
    private readonly Rectangle _nameBox = new(200, 270, 400, 80);
    private readonly Rectangle _nameConfirmBtn = new(200, 380, 400, 60);

    private readonly Rectangle _btnEasy = new(100, 200, 600, 65);
    private readonly Rectangle _btnNormal = new(100, 300, 600, 65);
    private readonly Rectangle _btnHard = new(100, 400, 600, 65);

    private readonly Rectangle _startTutorialBtn = new(200, 580, 400, 60);
    private readonly Rectangle _startPlayingButton = new(200, 620, 400, 50);

    private readonly Rectangle _checkButton = new(60, 620, 150, 50);
    private readonly Rectangle _resetButton = new(230, 620, 150, 50);
    private readonly Rectangle _peekButton = new(400, 620, 150, 50);
    private readonly Rectangle _menuButton = new(570, 620, 170, 50);

    // Shared by the Victory/GameOver screens and the ModeVerdict screen.
    private readonly Rectangle _nextGameButton = new(150, 500, 500, 60);
    private readonly Rectangle _victoryMenuButton = new(150, 580, 500, 60);

    private static readonly Dictionary<char, byte[]> FontData = new()
    {
        { 'A', new byte[] { 0x0E, 0x11, 0x11, 0x1F, 0x11, 0x11, 0x11 } },
        { 'B', new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x11, 0x11, 0x1E } },
        { 'C', new byte[] { 0x0E, 0x11, 0x10, 0x10, 0x10, 0x11, 0x0E } },
        { 'D', new byte[] { 0x1C, 0x12, 0x11, 0x11, 0x11, 0x12, 0x1C } },
        { 'E', new byte[] { 0x1F, 0x10, 0x10, 0x1E, 0x10, 0x10, 0x1F } },
        { 'F', new byte[] { 0x1F, 0x10, 0x10, 0x1E, 0x10, 0x10, 0x10 } },
        { 'G', new byte[] { 0x0E, 0x11, 0x10, 0x13, 0x11, 0x11, 0x0F } },
        { 'H', new byte[] { 0x11, 0x11, 0x11, 0x1F, 0x11, 0x11, 0x11 } },
        { 'I', new byte[] { 0x0E, 0x04, 0x04, 0x04, 0x04, 0x04, 0x0E } },
        { 'J', new byte[] { 0x07, 0x02, 0x02, 0x02, 0x02, 0x12, 0x0C } },
        { 'K', new byte[] { 0x11, 0x12, 0x14, 0x18, 0x14, 0x12, 0x11 } },
        { 'L', new byte[] { 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x1F } },
        { 'M', new byte[] { 0x11, 0x1B, 0x15, 0x11, 0x11, 0x11, 0x11 } },
        { 'N', new byte[] { 0x11, 0x11, 0x19, 0x15, 0x13, 0x11, 0x11 } },
        { 'O', new byte[] { 0x0E, 0x11, 0x11, 0x11, 0x11, 0x11, 0x0E } },
        { 'P', new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x10, 0x10, 0x10 } },
        { 'Q', new byte[] { 0x0E, 0x11, 0x11, 0x11, 0x15, 0x12, 0x0D } },
        { 'R', new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x14, 0x12, 0x11 } },
        { 'S', new byte[] { 0x0E, 0x11, 0x10, 0x0E, 0x01, 0x11, 0x0E } },
        { 'T', new byte[] { 0x1F, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04 } },
        { 'U', new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x0E } },
        { 'V', new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x0A, 0x04 } },
        { 'W', new byte[] { 0x11, 0x11, 0x11, 0x15, 0x15, 0x1B, 0x11 } },
        { 'X', new byte[] { 0x11, 0x11, 0x0A, 0x04, 0x0A, 0x11, 0x11 } },
        { 'Y', new byte[] { 0x11, 0x11, 0x0A, 0x04, 0x04, 0x04, 0x04 } },
        { 'Z', new byte[] { 0x1F, 0x01, 0x02, 0x04, 0x08, 0x10, 0x1F } },
        { '0', new byte[] { 0x0E, 0x11, 0x13, 0x15, 0x19, 0x11, 0x0E } },
        { '1', new byte[] { 0x04, 0x0C, 0x04, 0x04, 0x04, 0x04, 0x0E } },
        { '2', new byte[] { 0x0E, 0x11, 0x01, 0x02, 0x04, 0x08, 0x1F } },
        { '3', new byte[] { 0x1F, 0x02, 0x04, 0x02, 0x01, 0x11, 0x0E } },
        { '4', new byte[] { 0x02, 0x06, 0x0A, 0x12, 0x1F, 0x02, 0x02 } },
        { '5', new byte[] { 0x1F, 0x10, 0x1E, 0x01, 0x01, 0x11, 0x0E } },
        { '6', new byte[] { 0x06, 0x08, 0x10, 0x1E, 0x11, 0x11, 0x0E } },
        { '7', new byte[] { 0x1F, 0x01, 0x02, 0x04, 0x08, 0x08, 0x08 } },
        { '8', new byte[] { 0x0E, 0x11, 0x11, 0x0E, 0x11, 0x11, 0x0E } },
        { '9', new byte[] { 0x0E, 0x11, 0x11, 0x0F, 0x01, 0x02, 0x0C } },
        { ' ', new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
        { '!', new byte[] { 0x04, 0x04, 0x04, 0x04, 0x04, 0x00, 0x04 } },
        { '?', new byte[] { 0x0E, 0x11, 0x01, 0x02, 0x04, 0x00, 0x04 } },
        { '(', new byte[] { 0x02, 0x04, 0x08, 0x08, 0x08, 0x04, 0x02 } },
        { ')', new byte[] { 0x08, 0x04, 0x02, 0x02, 0x02, 0x04, 0x08 } },
        { ':', new byte[] { 0x00, 0x0C, 0x0C, 0x00, 0x0C, 0x0C, 0x00 } },
        { '-', new byte[] { 0x00, 0x00, 0x00, 0x1F, 0x00, 0x00, 0x00 } },
        { '.', new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x0C } },
        { ',', new byte[] { 0x00, 0x00, 0x00, 0x00, 0x0C, 0x04, 0x08 } },
        { '\'', new byte[] { 0x0C, 0x04, 0x08, 0x00, 0x00, 0x00, 0x00 } },
        { '/', new byte[] { 0x01, 0x02, 0x02, 0x04, 0x08, 0x08, 0x10 } },
        { '|', new byte[] { 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04 } },
        { '%', new byte[] { 0x18, 0x19, 0x02, 0x04, 0x08, 0x13, 0x03 } },
        { '+', new byte[] { 0x00, 0x04, 0x04, 0x1F, 0x04, 0x04, 0x00 } },
        { '*', new byte[] { 0x00, 0x15, 0x0E, 0x1F, 0x0E, 0x15, 0x00 } },
        { '=', new byte[] { 0x00, 0x00, 0x1F, 0x00, 0x1F, 0x00, 0x00 } },
        { '<', new byte[] { 0x02, 0x04, 0x08, 0x10, 0x08, 0x04, 0x02 } },
        { '>', new byte[] { 0x08, 0x04, 0x02, 0x01, 0x02, 0x04, 0x08 } },
        { '&', new byte[] { 0x0C, 0x12, 0x14, 0x08, 0x15, 0x12, 0x0D } },
        { '#', new byte[] { 0x0A, 0x0A, 0x1F, 0x0A, 0x1F, 0x0A, 0x0A } },
        { '_', new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F } }
    };

    public MonoGameApp(MainActivity activity)
    {
        _activity = activity;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        RollNewPreGameRoast();
    }

    private void RollNewPreGameRoast()
    {
        string[] roasts = {
            "WARNING: YOUR SHORT TERM MEMORY IS ABOUT TO BE EXPOSED",
            "PREPARE TO QUESTION EVERY LIFE CHOICE YOUVE MADE",
            "EXCUSE ME, DO YOU EVEN HAVE WORKING BRAIN CELLS?",
            "THIS GAME WILL PROVE YOUR DEGREE WAS A FLUKE",
            "ENTER AT YOUR OWN RISK. IGNORANCE WONT SAVE YOU"
        };
        _currentPreGameRoast = roasts[_random.Next(roasts.Length)];
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();

        TouchPanel.EnabledGestures = GestureType.Tap;

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        for (int i = 0; i < 60; i++)
        {
            _particles.Add(new BgParticle
            {
                Position = new Vector2((float)_random.NextDouble() * TargetWidth, (float)_random.NextDouble() * TargetHeight),
                Velocity = new Vector2((float)(_random.NextDouble() * 1.6 - 0.8), (float)(_random.NextDouble() * -2.0 - 0.4)),
                Size = (float)(_random.NextDouble() * 4 + 2),
                Color = new Color(_random.Next(20, 80), _random.Next(100, 220), _random.Next(180, 255), 180)
            });
        }

        CalculateScaleAndOffset();
        base.Initialize();
    }

    /// <summary>
    /// Fits the 800x700 virtual canvas INSIDE the safe area (status bar, taskbar, nav bar,
    /// camera cut-out) so the blue border can never be clipped on phones or tablets.
    /// </summary>
    private void CalculateScaleAndOffset()
    {
        float screenW = GraphicsDevice.Viewport.Width;
        float screenH = GraphicsDevice.Viewport.Height;

        int[] raw = _activity.GetSafeInsets(); // left, top, right, bottom in view pixels
        float viewW = _activity.GameViewWidth;
        float k = viewW > 0 ? screenW / viewW : 1f; // view pixels -> backbuffer pixels
        float margin = Math.Min(screenW, screenH) * 0.02f;

        float l = raw[0] * k + margin;
        float t = raw[1] * k + margin;
        float r = raw[2] * k + margin;
        float b = raw[3] * k + margin;

        float availW = Math.Max(1f, screenW - l - r);
        float availH = Math.Max(1f, screenH - t - b);

        _scale = Math.Min(availW / TargetWidth, availH / TargetHeight);

        float vpWidth = TargetWidth * _scale;
        float vpHeight = TargetHeight * _scale;

        _screenOffset = new Vector2(l + (availW - vpWidth) * 0.5f, t + (availH - vpHeight) * 0.5f);
    }

    private Vector2 ScreenToVirtual(Vector2 screenPos)
    {
        return (screenPos - _screenOffset) / _scale;
    }

    /// <summary>Keeps the native text field glued to the yellow box.</summary>
    private void SyncNameField()
    {
        int x = (int)(_screenOffset.X + _nameBox.X * _scale);
        int y = (int)(_screenOffset.Y + _nameBox.Y * _scale);
        int w = (int)(_nameBox.Width * _scale);
        int h = (int)(_nameBox.Height * _scale);
        _activity.ShowNameField(x, y, w, h, GraphicsDevice.Viewport.Width, 22f * _scale);
    }

    private void StartMode(GameMode mode)
    {
        _currentMode = mode;
        _currentLevel = 1;
        _currentGame = 1;
        _maxLevels = mode switch { GameMode.Easy => 5, GameMode.Normal => 10, _ => 20 };
        _maxTimeForLevel = mode switch { GameMode.Easy => 30f, GameMode.Normal => 20f, _ => 15f };
        BuildModeVerdict(mode);
        _currentState = GameState.ModeVerdict;
    }

    private void BuildModeVerdict(GameMode mode)
    {
        string n = GetPlayerName();
        string[] pool;
        switch (mode)
        {
            case GameMode.Easy:
                _verdictTitle = "EASY MODE? REALLY?";
                pool = new[] {
                    "{n}, YOU PICKED EASY. EVEN THE TUTORIAL IS EMBARRASSED FOR YOU.",
                    "TRAINING WHEELS ENGAGED. MOMMY WILL BE SO PROUD, {n}.",
                    "EASY MODE: BECAUSE {n} KNOWS EXACTLY WHAT THEY CAN HANDLE. NOTHING.",
                    "WOW. A TODDLER WOULD PICK NORMAL. BUT SURE, {n}. EASY."
                };
                _verdictStats = new[] { "COURAGE: 2%", "BRAIN CELLS REQUIRED: 3", "EXPECTED TEARS: MOSTLY SHAME" };
                break;
            case GameMode.Normal:
                _verdictTitle = "NORMAL. HOW MEDIOCRE.";
                pool = new[] {
                    "AVERAGE CHOICE FOR AN AVERAGE BRAIN, {n}. WE ARE NOT SURPRISED.",
                    "NORMAL? THE MOST BEIGE OPTION POSSIBLE. THAT IS SO YOU, {n}.",
                    "{n} PICKED THE MIDDLE ONE. SO BRAVE. SO UNMEMORABLE."
                };
                _verdictStats = new[] { "COURAGE: 50% (GENEROUS)", "BRAIN CELLS REQUIRED: 7", "EXPECTED TEARS: A FEW" };
                break;
            default:
                _verdictTitle = "HARD? DELUSIONAL.";
                pool = new[] {
                    "{n}, YOU THINK YOU ARE SMART? THE GRID DISAGREES. LOUDLY.",
                    "HARD MODE FOR {n}? WE WILL BRING TISSUES AND A CAMERA.",
                    "BOLD. FOOLISH. DOOMED. THAT IS A LOT OF CONFIDENCE FOR SOMEONE WHO FORGOT WHERE THEY PARKED."
                };
                _verdictStats = new[] { "COURAGE: 100% (BRAINS: 0%)", "BRAIN CELLS REQUIRED: ALL OF THEM", "EXPECTED TEARS: A RIVER" };
                break;
        }
        _verdictText = pool[_random.Next(pool.Length)].Replace("{n}", n);
    }

    private void LoadGameStage(int level, int game)
    {
        _currentLevel = level;
        _currentGame = game;
        _timeRemaining = _maxTimeForLevel;
        _peekTimer = 0f;
        _statusMessage = "";
        _wrongChecks = 0;
        _peeksUsed = 0;
        _cellsWiped = 0;
        _targetSolutionPath.Clear();
        _targetSolutionSet.Clear();
        _userDrawnPathSet.Clear();

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

        foreach (var node in _targetSolutionPath)
        {
            if ((node.X == 2 && node.Y == 10) || (node.X == 18 && node.Y == 10)) continue;
            _targetSolutionSet.Add((node.X, node.Y));
        }

        _currentState = GameState.Memorizing;
    }

    private bool ValidateExactUserPath()
    {
        if (_userDrawnPathSet.Count != _targetSolutionSet.Count) return false;
        foreach (var cell in _userDrawnPathSet)
            if (!_targetSolutionSet.Contains(cell)) return false;
        return true;
    }

    private void TriggerVictory()
    {
        string name = GetPlayerName();
        string[] titles = { "OH, YOU ACTUALLY WON?", "PURE BLIND LUCK!", "IMPOSSIBLE MIRACLE!" };
        string[] subtexts = {
            $"{name}, YOU TRACED A LINE WITHOUT DROOLING!",
            $"DON'T LET THIS GO TO YOUR FRAGILE HEAD, {name}.",
            $"EVEN A GOLDFISH MEMORIZES BETTER, BUT WE ACCEPT IT."
        };

        _currentVictoryTitle = titles[_random.Next(titles.Length)];
        _currentVictorySubtext = subtexts[_random.Next(subtexts.Length)];
        FinishRound(true);
        _currentState = GameState.Victory;
    }

    private void TriggerGameOver()
    {
        string name = GetPlayerName();
        string[] titles = { "TIME IS UP, COWARD!", "GOLDFISH MEMORY CAP!", "BRAIN CELL OVERLOAD!" };
        string[] subtexts = {
            $"{name}'S BRAIN CELLS FAILED TO CONNECT IN TIME!",
            $"THAT PATH WAS ON SCREEN FOR SECONDS. DID {name} BLINK?",
            $"WE WOULD JUDGE YOU, BUT THIS FAILURE SPEAKS FOR ITSELF."
        };

        _currentGameOverTitle = titles[_random.Next(titles.Length)];
        _currentGameOverSubtext = subtexts[_random.Next(subtexts.Length)];
        FinishRound(false);
        _currentState = GameState.GameOver;
    }

    /// <summary>Computes the stats, shame, achievement, insults and confetti for the result screen.</summary>
    private void FinishRound(bool win)
    {
        string n = GetPlayerName();
        _resultTime = 0f;
        _timeUsed = win ? _maxTimeForLevel - _timeRemaining : _maxTimeForLevel;
        _shame = Math.Clamp(_wrongChecks * 18 + _peeksUsed * 14 + _cellsWiped + (win ? 0 : 25), 5, 100);

        if (win)
        {
            if (_wrongChecks == 0 && _peeksUsed == 0) _achievement = "ACHIEVEMENT: SUSPICIOUSLY GOOD (CHEATER?)";
            else if (_peeksUsed >= 2) _achievement = "ACHIEVEMENT: PROFESSIONAL PEEKER";
            else if (_wrongChecks >= 3) _achievement = "ACHIEVEMENT: PERSISTENT TO THE POINT OF PAIN";
            else _achievement = "ACHIEVEMENT: PARTICIPATION TROPHY";

            _resultInsults = new[] {
                "YOUR PARENTS SAID YOU WERE SPECIAL. THEY LIED.",
                $"{n}, THE GRID FELT SORRY FOR YOU AND LET YOU WIN.",
                "THAT WAS A FLUKE. WE ARE CALLING IT A FLUKE.",
                "ENJOY THIS MOMENT. IT WILL NEVER HAPPEN AGAIN.",
                "WOW. A WHOLE LINE. WHAT A LEGEND. (NOT)"
            };
        }
        else
        {
            if (_wrongChecks == 0) _achievement = "ACHIEVEMENT: DIDN'T EVEN TRY";
            else if (_peeksUsed >= 2) _achievement = "ACHIEVEMENT: PEEKED AND STILL FAILED";
            else if (_wrongChecks >= 3) _achievement = "ACHIEVEMENT: WRONG ANSWER SPECIALIST";
            else _achievement = "ACHIEVEMENT: GOLDFISH OF THE YEAR";

            _resultInsults = new[] {
                "A SQUIRREL REMEMBERS 10000 BURIED NUTS. YOU CAN'T REMEMBER 12 CELLS.",
                $"{n}, YOUR BRAIN BUFFERED SO HARD IT TIMED OUT.",
                "WE SHOWED YOU THE PATH. WE EVEN LIT IT UP IN BLUE. TRAGIC.",
                "SOMEWHERE, A GOLDFISH IS LAUGHING AT YOU.",
                "TRY AGAIN. OR DON'T. THE GRID WILL JUDGE EITHER WAY."
            };
        }

        _confetti.Clear();
        string glyphs = win ? "W*+W" : "LXLX";
        for (int i = 0; i < 45; i++)
        {
            bool alt = i % 2 == 0;
            _confetti.Add(new Confetti
            {
                Position = new Vector2((float)_random.NextDouble() * TargetWidth, -(float)_random.NextDouble() * TargetHeight),
                Velocity = new Vector2((float)(_random.NextDouble() - 0.5), (float)(_random.NextDouble() * 2.5 + 1.2)),
                Glyph = glyphs[_random.Next(glyphs.Length)],
                Scale = _random.Next(2, 5),
                Color = win ? (alt ? Color.LimeGreen : Color.Gold) : (alt ? Color.Crimson : Color.OrangeRed)
            });
        }
    }

    private string GetPlayerName() => string.IsNullOrWhiteSpace(_userName) ? "UNKNOWN VICTIM" : _userName.ToUpper();

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Re-fit to the safe area twice a second (rotation, taskbar, cut-outs...).
        _layoutTimer += dt;
        if (_layoutTimer >= 0.5f)
        {
            _layoutTimer = 0f;
            CalculateScaleAndOffset();
            if (_currentState == GameState.NameInput) SyncNameField();
        }

        for (int i = 0; i < _particles.Count; i++)
        {
            var p = _particles[i];
            p.Position += p.Velocity;
            if (p.Position.Y < -10) p.Position.Y = TargetHeight + 10;
            if (p.Position.X < -10) p.Position.X = TargetWidth + 10;
            if (p.Position.X > TargetWidth + 10) p.Position.X = -10;
            _particles[i] = p;
        }

        if (_currentState == GameState.Victory || _currentState == GameState.GameOver)
        {
            _resultTime += dt;
            for (int i = 0; i < _confetti.Count; i++)
            {
                var c = _confetti[i];
                c.Position += c.Velocity * (dt * 60f);
                if (c.Position.Y > TargetHeight + 20)
                {
                    c.Position.Y = -20;
                    c.Position.X = (float)_random.NextDouble() * TargetWidth;
                }
                _confetti[i] = c;
            }
        }

        if (_statusMessageTimer > 0)
        {
            _statusMessageTimer -= dt;
            if (_statusMessageTimer <= 0) _statusMessage = "";
        }

        if (_peekTimer > 0)
        {
            _peekTimer -= dt;
            if (_peekTimer <= 0) _peekTimer = 0f;
        }

        if (_currentState == GameState.Playing)
        {
            _timeRemaining -= dt;
            if (_timeRemaining <= 0)
            {
                _timeRemaining = 0;
                TriggerGameOver();
            }
        }

        var touchState = TouchPanel.GetState();
        if (touchState.Count > 0)
        {
            var touch = touchState[0];
            Vector2 virtualPos = ScreenToVirtual(touch.Position);
            Point posPoint = new((int)virtualPos.X, (int)virtualPos.Y);

            if (touch.State == TouchLocationState.Pressed)
            {
                if (_currentState == GameState.NameInput)
                {
                    // The box itself is a real EditText now, so only the button is handled here.
                    if (_nameConfirmBtn.Contains(posPoint))
                    {
                        _activity.HideKeyboard();
                        _activity.ShowNameVerdict(_activity.NameFieldText.Trim(), finalName =>
                        {
                            _userName = finalName;
                            _currentState = GameState.MainMenu;
                            _activity.RemoveNameField();
                        });
                    }
                }
                else if (_currentState == GameState.MainMenu)
                {
                    if (_btnEasy.Contains(posPoint)) StartMode(GameMode.Easy);
                    else if (_btnNormal.Contains(posPoint)) StartMode(GameMode.Normal);
                    else if (_btnHard.Contains(posPoint)) StartMode(GameMode.Hard);
                }
                else if (_currentState == GameState.ModeVerdict)
                {
                    if (_nextGameButton.Contains(posPoint)) _currentState = GameState.Tutorial;
                    else if (_victoryMenuButton.Contains(posPoint)) _currentState = GameState.MainMenu;
                }
                else if (_currentState == GameState.Tutorial && _startTutorialBtn.Contains(posPoint))
                {
                    LoadGameStage(_currentLevel, _currentGame);
                }
                else if (_currentState == GameState.Memorizing && _startPlayingButton.Contains(posPoint))
                {
                    _currentState = GameState.Playing;
                }
                else if (_currentState == GameState.Playing)
                {
                    if (_checkButton.Contains(posPoint))
                    {
                        if (ValidateExactUserPath()) TriggerVictory();
                        else
                        {
                            _wrongChecks++;
                            string[] roasts = {
                                $"EPIC FAIL, {GetPlayerName()}! LOOKS LIKE A TODDLER SCRIBBLE!",
                                $"ARE YOUR EYES PAINTED ON, {GetPlayerName()}?",
                                $"NOT EVEN CLOSE! TRY USING YOUR BRAIN THIS TIME!"
                            };
                            _statusMessage = roasts[_random.Next(roasts.Length)];
                            _statusMessageTimer = 3.0f;
                        }
                    }
                    else if (_resetButton.Contains(posPoint))
                    {
                        _cellsWiped += _userDrawnPathSet.Count;
                        _userDrawnPathSet.Clear();
                    }
                    else if (_peekButton.Contains(posPoint))
                    {
                        _peeksUsed++;
                        _peekTimer = 2.0f;
                        _timeRemaining = Math.Max(1f, _timeRemaining - 5f);
                    }
                    else if (_menuButton.Contains(posPoint))
                    {
                        _activity.ShowSurrenderDialog(() =>
                        {
                            _currentState = GameState.MainMenu;
                        });
                    }
                }
                else if ((_currentState == GameState.Victory || _currentState == GameState.GameOver) && _nextGameButton.Contains(posPoint))
                {
                    int nextGame = _currentGame + 1;
                    int nextLevel = _currentLevel;
                    if (nextGame > GamesPerLevel) { nextGame = 1; nextLevel++; if (nextLevel > _maxLevels) nextLevel = 1; }
                    LoadGameStage(nextLevel, nextGame);
                }
                else if ((_currentState == GameState.Victory || _currentState == GameState.GameOver) && _victoryMenuButton.Contains(posPoint))
                {
                    _currentState = GameState.MainMenu;
                }
            }

            if (_currentState == GameState.Playing && (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved))
            {
                int gx = (int)((virtualPos.X - GridOffsetX) / CellSize);
                int gy = (int)((virtualPos.Y - GridOffsetY) / CellSize);
                if (virtualPos.X >= GridOffsetX && virtualPos.Y >= GridOffsetY && gx >= 0 && gx < GridSize && gy >= 0 && gy < GridSize)
                {
                    if (_grid[gx, gy].IsWalkable && !((gx == 2 && gy == 10) || (gx == 18 && gy == 10)))
                    {
                        var cell = (gx, gy);
                        if (touch.State == TouchLocationState.Pressed && _userDrawnPathSet.Contains(cell))
                            _userDrawnPathSet.Remove(cell);
                        else
                            _userDrawnPathSet.Add(cell);
                    }
                }
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Same colour as the canvas so the letterbox area blends in.
        GraphicsDevice.Clear(new Color(15, 23, 42));

        Matrix transform = Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_screenOffset.X, _screenOffset.Y, 0);
        _spriteBatch.Begin(transformMatrix: transform);

        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, TargetWidth, TargetHeight), new Color(15, 23, 42));
        DrawHollowRect(new Rectangle(10, 10, TargetWidth - 20, TargetHeight - 20), 3, new Color(56, 189, 248));

        foreach (var p in _particles)
            _spriteBatch.Draw(_pixel, new Rectangle((int)p.Position.X, (int)p.Position.Y, (int)p.Size, (int)p.Size), p.Color);

        string activePlayer = GetPlayerName();

        if (_currentState == GameState.NameInput)
        {
            DrawCenteredPixelString("MEMORY ROASTER 3000", 70, 3, Color.SkyBlue);
            DrawCenteredPixelString("WHO IS DARING TO SUFFER?", 130, 2, Color.Gold);

            DrawFittedCentered(_currentPreGameRoast, 190, 1, Color.Crimson);
            DrawCenteredPixelString("REGISTER YOUR EGO FOR PUBLIC JUDGMENT BELOW", 225, 1, Color.White);

            // The native EditText sits on top of this box.
            _spriteBatch.Draw(_pixel, _nameBox, new Color(30, 41, 59));
            DrawHollowRect(_nameBox, 3, Color.Gold);

            DrawButton(_nameConfirmBtn, Color.Green, "REGISTER INEVITABLE FAILURE", 2);

            DrawFittedCentered("TIP: LEAVE IT BLANK AND WE WILL PICK A NAME FOR YOU. YOU WILL REGRET IT.", 470, 1, Color.DarkGray);
        }
        else if (_currentState == GameState.MainMenu)
        {
            DrawCenteredPixelString("MEMORY ROASTER 3000", 50, 3, Color.SkyBlue);
            DrawFittedCentered($"TARGET PLAYER: {activePlayer}", 105, 2, Color.Gold);
            DrawCenteredPixelString("SELECT HOW FAST YOU WANT TO CRY:", 145, 2, Color.White);

            DrawButton(_btnEasy, Color.Green, "EASY (FOR 3 BRAIN CELLS)", 2);
            DrawButton(_btnNormal, Color.Gold, "NORMAL (YOU WILL FAIL LEVEL 1)", 2);
            DrawButton(_btnHard, Color.Crimson, "HARD (DELETE APP & CRY TO SLEEP)", 2);

            DrawCenteredPixelString("GRIDFORGE OS V3.0 - NO MERCY EDITION", 645, 2, Color.DarkGray);
        }
        else if (_currentState == GameState.ModeVerdict)
        {
            Color mc = _currentMode switch { GameMode.Easy => Color.Green, GameMode.Normal => Color.Gold, _ => Color.Crimson };
            DrawCenteredPixelString("DIFFICULTY VERDICT", 50, 3, Color.SkyBlue);

            Rectangle badge = new(200, 110, 400, 100);
            _spriteBatch.Draw(_pixel, badge, mc);
            DrawHollowRect(new Rectangle(badge.X - 3, badge.Y - 3, badge.Width + 6, badge.Height + 6), 3, Color.White);
            string modeName = _currentMode.ToString().ToUpper();
            DrawCenteredPixelString(modeName, badge.Y + (badge.Height - 42) / 2, 6, Color.White);

            DrawFittedCentered(_verdictTitle, 235, 3, Color.Gold);
            DrawWrapped(_verdictText, 285, 2, Color.White, 680);

            for (int i = 0; i < _verdictStats.Length; i++)
                DrawFittedCentered(_verdictStats[i], 385 + i * 28, 2, Color.OrangeRed);

            DrawButton(_nextGameButton, Color.DarkGreen, "FINE. I ACCEPT MY SHAME", 2);
            DrawButton(_victoryMenuButton, Color.Maroon, "WAIT, LET ME PICK AGAIN", 2);
        }
        else if (_currentState == GameState.Tutorial)
        {
            DrawFittedCentered($"WELCOME, {activePlayer}!", 50, 3, Color.SkyBlue);
            DrawCenteredPixelString("LESSON 1: HOW TO NOT EMBARRASS YOURSELF", 100, 2, Color.Gold);

            string[] steps = {
                "1. STARE AT THE BLUE LINE. TRY NOT TO DROOL ON SCREEN.",
                "2. TAP START TO HIDE IT BEFORE PANIC SETS IN.",
                "3. DRAG YOUR FINGER ACROSS CELLS TO REDRAW IT.",
                "4. TAP ANY CELL AGAIN TO ERASE YOUR SHAMEFUL MISTAKES.",
                "5. IF YOU FAIL, WE WILL JUDGE YOU LOUDLY AND PUBLICLY."
            };

            for (int i = 0; i < steps.Length; i++)
                DrawFittedCentered(steps[i], 180 + (i * 65), 2, Color.White);

            DrawButton(_startTutorialBtn, Color.DarkGreen, "I DARE TO TRY (PROVE ME WRONG)", 2);
        }
        else if (_currentState == GameState.Memorizing || _currentState == GameState.Playing)
        {
            DrawFittedCentered($"PLAYER: {activePlayer} | {_currentMode} LVL {_currentLevel}/{_maxLevels}", 18, 2, Color.LightGray);

            if (_currentState == GameState.Playing)
            {
                float pct = _timeRemaining / _maxTimeForLevel;
                Color timerCol = pct > 0.4f ? Color.LimeGreen : Color.Red;
                _spriteBatch.Draw(_pixel, new Rectangle(GridOffsetX, 48, (int)(500 * pct), 16), timerCol);
                DrawHollowRect(new Rectangle(GridOffsetX, 48, 500, 16), 2, Color.White);
            }

            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    var node = _grid[x, y];
                    Color cellColor = node.IsWalkable ? new Color(226, 232, 240) : new Color(71, 85, 105);

                    bool showSolution = (_currentState == GameState.Memorizing) || (_peekTimer > 0f);

                    if (showSolution && _targetSolutionPath.Contains(node))
                        cellColor = new Color(56, 189, 248);
                    else if (_currentState == GameState.Playing && _userDrawnPathSet.Contains((x, y)))
                        cellColor = new Color(56, 189, 248);

                    _spriteBatch.Draw(_pixel, new Rectangle(GridOffsetX + x * CellSize, GridOffsetY + y * CellSize, CellSize - 2, CellSize - 2), cellColor);
                }
            }

            _spriteBatch.Draw(_pixel, new Rectangle(GridOffsetX + 2 * CellSize, GridOffsetY + 10 * CellSize, CellSize - 2, CellSize - 2), Color.LimeGreen);
            _spriteBatch.Draw(_pixel, new Rectangle(GridOffsetX + 18 * CellSize, GridOffsetY + 10 * CellSize, CellSize - 2, CellSize - 2), Color.Red);

            if (_currentState == GameState.Memorizing)
            {
                DrawButton(_startPlayingButton, Color.DarkGreen, "START DRAWING (HIDE SOLUTION)", 2);
            }
            else
            {
                DrawButton(_checkButton, Color.DarkGreen, "CHECK WORK", 2);
                DrawButton(_resetButton, Color.DarkBlue, "WIPE SHAME", 2);
                DrawButton(_peekButton, Color.DarkGoldenrod, "PEEK (-5S)", 2);
                DrawButton(_menuButton, Color.Maroon, "GIVE UP", 2);

                // Sits in the gap between the grid (ends y=600) and the buttons (start y=617).
                if (!string.IsNullOrEmpty(_statusMessage))
                    DrawFittedCentered(_statusMessage, 602, 2, Color.Red, 760);
            }
        }
        else if (_currentState == GameState.Victory || _currentState == GameState.GameOver)
        {
            bool win = _currentState == GameState.Victory;
            Color bannerCol = win ? Color.LimeGreen : Color.Crimson;
            string title = win ? _currentVictoryTitle : _currentGameOverTitle;
            string sub = win ? _currentVictorySubtext : _currentGameOverSubtext;

            // falling W's (win) or L's (lose), drawn behind the UI
            foreach (var c in _confetti)
                DrawPixelString(c.Glyph.ToString(), (int)c.Position.X, (int)c.Position.Y, c.Scale, c.Color * 0.55f);

            DrawFittedCentered(title, 45, 3, bannerCol);
            DrawWrapped(sub, 88, 2, Color.White, 720);

            // --- stats + hall of shame ---
            Rectangle left = new(60, 150, 330, 150);
            Rectangle right = new(410, 150, 330, 150);
            foreach (var panel in new[] { left, right })
            {
                _spriteBatch.Draw(_pixel, panel, new Color(30, 41, 59));
                DrawHollowRect(panel, 3, Color.SkyBlue);
            }

            DrawPixelString("YOUR STATS", left.X + 14, left.Y + 10, 2, Color.Gold);
            string[] stats = {
                $"TIME USED: {_timeUsed:0}S",
                $"WRONG CHECKS: {_wrongChecks}",
                $"PEEKS USED: {_peeksUsed}",
                $"CELLS WIPED: {_cellsWiped}"
            };
            for (int i = 0; i < stats.Length; i++)
                DrawPixelString(stats[i], left.X + 14, left.Y + 38 + i * 26, 2, Color.White);

            DrawPixelString("HALL OF SHAME", right.X + 14, right.Y + 10, 2, Color.Gold);
            string shortName = activePlayer.Length > 12 ? activePlayer.Substring(0, 12) : activePlayer;
            string[] board = { "1. SLEEPING CAT", "2. A POTATO", "3. WIFI ROUTER", $"4. {shortName}" };
            for (int i = 0; i < board.Length; i++)
                DrawPixelString(board[i], right.X + 14, right.Y + 38 + i * 26, 2, i == 3 ? Color.Crimson : Color.LightGray);

            // --- embarrassment meter ---
            DrawCenteredPixelString($"EMBARRASSMENT LEVEL: {_shame}%", 322, 2, Color.White);
            Rectangle bar = new(150, 345, 500, 22);
            float anim = Math.Min(1f, _resultTime / 1.2f);
            Color shameCol = _shame < 40 ? Color.LimeGreen : (_shame < 70 ? Color.Gold : Color.Red);
            _spriteBatch.Draw(_pixel, new Rectangle(bar.X, bar.Y, (int)(bar.Width * (_shame / 100f) * anim), bar.Height), shameCol);
            DrawHollowRect(bar, 2, Color.White);

            DrawFittedCentered(_achievement, 385, 2, Color.Gold);

            if (_resultInsults.Length > 0)
            {
                int idx = (int)(_resultTime / 2.5f) % _resultInsults.Length;
                DrawWrapped(_resultInsults[idx], 422, 2, Color.OrangeRed, 720);
            }

            DrawButton(_nextGameButton, Color.DarkBlue, win ? "SUBJECT YOURSELF TO MORE TORTURE" : "EMBARRASS YOURSELF AGAIN", 2);
            DrawButton(_victoryMenuButton, Color.Maroon, "RUN AWAY TO MENU LIKE A COWARD", 2);
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawHollowRect(Rectangle rect, int borderWidth, Color color)
    {
        _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, borderWidth), color);
        _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y + rect.Height - borderWidth, rect.Width, borderWidth), color);
        _spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, borderWidth, rect.Height), color);
        _spriteBatch.Draw(_pixel, new Rectangle(rect.X + rect.Width - borderWidth, rect.Y, borderWidth, rect.Height), color);
    }

    /// <summary>
    /// Draws the label centred INSIDE the button rectangle (the old version centred on the whole
    /// screen, which is why every label ended up stacked in the middle). Shrinks long labels to fit.
    /// </summary>
    private void DrawButton(Rectangle rect, Color color, string label, int scale)
    {
        DrawHollowRect(new Rectangle(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6), 3, Color.White);
        _spriteBatch.Draw(_pixel, rect, color);

        int s = scale;
        while (s > 1 && label.Length * 6 * s > rect.Width - 16) s--;
        int textWidth = label.Length * 6 * s;
        DrawPixelString(label, rect.X + (rect.Width - textWidth) / 2, rect.Y + (rect.Height - 7 * s) / 2, s, Color.White);
    }

    private void DrawCenteredPixelString(string text, int y, int scale, Color color)
    {
        int textWidth = text.Length * (6 * scale);
        int x = (TargetWidth - textWidth) / 2;
        DrawPixelString(text, x, y, scale, color);
    }

    /// <summary>Centred text that automatically drops to a smaller scale if it would overflow.</summary>
    private void DrawFittedCentered(string text, int y, int maxScale, Color color, int maxWidth = TargetWidth - 60)
    {
        int s = maxScale;
        while (s > 1 && text.Length * 6 * s > maxWidth) s--;
        DrawCenteredPixelString(text, y, s, color);
    }

    private List<string> WrapText(string text, int scale, int maxWidth)
    {
        int maxChars = Math.Max(1, maxWidth / (6 * scale));
        var lines = new List<string>();
        string cur = "";
        foreach (var word in text.Split(' '))
        {
            if (cur.Length == 0) cur = word;
            else if (cur.Length + 1 + word.Length <= maxChars) cur += " " + word;
            else { lines.Add(cur); cur = word; }
        }
        if (cur.Length > 0) lines.Add(cur);
        return lines;
    }

    /// <summary>Centred, word-wrapped text. Returns the Y just below the last line.</summary>
    private int DrawWrapped(string text, int y, int scale, Color color, int maxWidth)
    {
        foreach (var line in WrapText(text, scale, maxWidth))
        {
            DrawCenteredPixelString(line, y, scale, color);
            y += 7 * scale + 6;
        }
        return y;
    }

    private void DrawPixelString(string text, int x, int y, int scale, Color color)
    {
        string upper = text.ToUpper();
        int curX = x;

        foreach (char c in upper)
        {
            if (FontData.TryGetValue(c, out var glyph))
            {
                for (int row = 0; row < 7; row++)
                {
                    byte b = glyph[row];
                    for (int col = 0; col < 5; col++)
                    {
                        if ((b & (1 << (4 - col))) != 0)
                        {
                            _spriteBatch.Draw(_pixel, new Rectangle(curX + col * scale, y + row * scale, scale, scale), color);
                        }
                    }
                }
            }
            curX += 6 * scale;
        }
    }
}