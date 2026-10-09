using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using GridForge.Core.AI.Pathfinding;

namespace GridForge.Android;

public enum GameMode { None, Easy, Normal, Hard }
public enum GameState { NameInput, MainMenu, Tutorial, Memorizing, Playing, Victory, GameOver }

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

    private GameState _currentState = GameState.NameInput;
    private GameMode _currentMode = GameMode.None;

    private string _userName = "KESHAV";
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

    private readonly Random _random = new();

    private struct BgParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;
        public Color Color;
    }
    private readonly List<BgParticle> _particles = new();

    private readonly Rectangle _nameConfirmBtn = new(200, 480, 400, 60);
    private readonly Rectangle _btnEasy = new(100, 200, 600, 65);
    private readonly Rectangle _btnNormal = new(100, 300, 600, 65);
    private readonly Rectangle _btnHard = new(100, 400, 600, 65);

    private readonly Rectangle _startTutorialBtn = new(200, 580, 400, 60);
    private readonly Rectangle _startPlayingButton = new(200, 620, 400, 50);

    private readonly Rectangle _checkButton = new(60, 620, 150, 50);
    private readonly Rectangle _resetButton = new(230, 620, 150, 50);
    private readonly Rectangle _peekButton = new(400, 620, 150, 50);
    private readonly Rectangle _menuButton = new(570, 620, 170, 50);

    private readonly Rectangle _nextGameButton = new(150, 460, 500, 60);
    private readonly Rectangle _victoryMenuButton = new(150, 540, 500, 60);

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
        { '\'', new byte[] { 0x0C, 0x04, 0x08, 0x00, 0x00, 0x00, 0x00 } }
    };

    public MonoGameApp(MainActivity activity)
    {
        _activity = activity;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
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

    private void CalculateScaleAndOffset()
    {
        float screenW = GraphicsDevice.Viewport.Width;
        float screenH = GraphicsDevice.Viewport.Height;

        float scaleX = screenW / TargetWidth;
        float scaleY = screenH / TargetHeight;

        _scale = Math.Min(scaleX, scaleY);

        float vpWidth = TargetWidth * _scale;
        float vpHeight = TargetHeight * _scale;

        _screenOffset = new Vector2((screenW - vpWidth) * 0.5f, (screenH - vpHeight) * 0.5f);
    }

    private Vector2 ScreenToVirtual(Vector2 screenPos)
    {
        return (screenPos - _screenOffset) / _scale;
    }

    private void StartMode(GameMode mode)
    {
        _currentMode = mode;
        _currentLevel = 1;
        _currentGame = 1;
        _maxLevels = mode switch { GameMode.Easy => 5, GameMode.Normal => 10, _ => 20 };
        _maxTimeForLevel = mode switch { GameMode.Easy => 30f, GameMode.Normal => 20f, _ => 15f };
        _currentState = GameState.Tutorial;
    }

    private void LoadGameStage(int level, int game)
    {
        _currentLevel = level;
        _currentGame = game;
        _timeRemaining = _maxTimeForLevel;
        _peekTimer = 0f;
        _statusMessage = "";
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
        _currentState = GameState.GameOver;
    }

    private string GetPlayerName() => string.IsNullOrWhiteSpace(_userName) ? "KESHAV" : _userName.ToUpper();

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        for (int i = 0; i < _particles.Count; i++)
        {
            var p = _particles[i];
            p.Position += p.Velocity;
            if (p.Position.Y < -10) p.Position.Y = TargetHeight + 10;
            if (p.Position.X < -10) p.Position.X = TargetWidth + 10;
            if (p.Position.X > TargetWidth + 10) p.Position.X = -10;
            _particles[i] = p;
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
                if (_currentState == GameState.NameInput && _nameConfirmBtn.Contains(posPoint))
                {
                    _activity.PromptForPlayerName(enteredName =>
                    {
                        _userName = enteredName;
                        _currentState = GameState.MainMenu;
                    });
                }
                else if (_currentState == GameState.MainMenu)
                {
                    if (_btnEasy.Contains(posPoint)) StartMode(GameMode.Easy);
                    else if (_btnNormal.Contains(posPoint)) StartMode(GameMode.Normal);
                    else if (_btnHard.Contains(posPoint)) StartMode(GameMode.Hard);
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
                            string[] roasts = {
                                $"EPIC FAIL, {GetPlayerName()}! LOOKS LIKE A TODDLER SCRIBBLE!",
                                $"ARE YOUR EYES PAINTED ON, {GetPlayerName()}?",
                                $"NOT EVEN CLOSE! TRY USING YOUR BRAIN THIS TIME!"
                            };
                            _statusMessage = roasts[_random.Next(roasts.Length)];
                            _statusMessageTimer = 3.0f;
                        }
                    }
                    else if (_resetButton.Contains(posPoint)) _userDrawnPathSet.Clear();
                    else if (_peekButton.Contains(posPoint))
                    {
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
                if (gx >= 0 && gx < GridSize && gy >= 0 && gy < GridSize)
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
        GraphicsDevice.Clear(Color.Black);

        Matrix transform = Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_screenOffset.X, _screenOffset.Y, 0);
        _spriteBatch.Begin(transformMatrix: transform);

        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, TargetWidth, TargetHeight), new Color(15, 23, 42));
        DrawHollowRect(new Rectangle(10, 10, TargetWidth - 20, TargetHeight - 20), 3, new Color(56, 189, 248));

        foreach (var p in _particles)
            _spriteBatch.Draw(_pixel, new Rectangle((int)p.Position.X, (int)p.Position.Y, (int)p.Size, (int)p.Size), p.Color);

        string activePlayer = GetPlayerName();

        if (_currentState == GameState.NameInput)
        {
            DrawCenteredPixelString("WHO IS DARING TO SUFFER?", 120, 3, Color.SkyBlue);
            DrawCenteredPixelString("REGISTER YOUR EGO FOR PUBLIC JUDGMENT:", 180, 2, Color.White);

            Rectangle box = new(200, 260, 400, 80);
            _spriteBatch.Draw(_pixel, box, new Color(30, 41, 59));
            DrawHollowRect(box, 3, Color.Gold);
            DrawCenteredPixelString($"PLAYER: {activePlayer}", 288, 3, Color.Gold);

            DrawButton(_nameConfirmBtn, Color.Green, "REGISTER INEVITABLE FAILURE");
        }
        else if (_currentState == GameState.MainMenu)
        {
            DrawCenteredPixelString("MEMORY ROASTER 3000", 50, 3, Color.SkyBlue);
            DrawCenteredPixelString($"TARGET PLAYER: {activePlayer}", 105, 2, Color.Gold);
            DrawCenteredPixelString("SELECT HOW FAST YOU WANT TO CRY:", 145, 2, Color.White);

            DrawButton(_btnEasy, Color.Green, "EASY (FOR 3 BRAIN CELLS)");
            DrawButton(_btnNormal, Color.Gold, "NORMAL (YOU WILL FAIL LEVEL 1)");
            DrawButton(_btnHard, Color.Crimson, "HARD (DELETE APP & CRY TO SLEEP)");

            DrawCenteredPixelString("GRIDFORGE OS v3.0 - NO MERCY EDITION", 645, 2, Color.DarkGray);
        }
        else if (_currentState == GameState.Tutorial)
        {
            DrawCenteredPixelString($"WELCOME, {activePlayer}!", 50, 3, Color.SkyBlue);
            DrawCenteredPixelString("LESSON 1: HOW TO NOT EMBARRASS YOURSELF", 100, 2, Color.Gold);

            string[] steps = {
                "1. STARE AT THE BLUE LINE. TRY NOT TO DROOL ON SCREEN.",
                "2. TAP START TO HIDE IT BEFORE PANIC SETS IN.",
                "3. DRAG YOUR FINGER ACROSS CELLS TO REDRAW IT.",
                "4. TAP ANY CELL AGAIN TO ERASE YOUR SHAMEFUL MISTAKES.",
                "5. IF YOU FAIL, WE WILL JUDGE YOU LOUDLY AND PUBLICLY."
            };

            for (int i = 0; i < steps.Length; i++)
                DrawCenteredPixelString(steps[i], 180 + (i * 65), 2, Color.White);

            DrawButton(_startTutorialBtn, Color.DarkGreen, "I DARE TO TRY (PROVE ME WRONG)");
        }
        else if (_currentState == GameState.Memorizing || _currentState == GameState.Playing)
        {
            DrawCenteredPixelString($"PLAYER: {activePlayer} | {_currentMode} LVL {_currentLevel}/{_maxLevels}", 18, 2, Color.LightGray);

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
                DrawButton(_startPlayingButton, Color.DarkGreen, "START DRAWING (HIDE SOLUTION)");
            }
            else
            {
                DrawButton(_checkButton, Color.DarkGreen, "CHECK WORK");
                DrawButton(_resetButton, Color.DarkBlue, "WIPE SHAME");
                DrawButton(_peekButton, Color.DarkGoldenrod, "PEEK (-5S)");
                DrawButton(_menuButton, Color.Maroon, "GIVE UP");

                if (!string.IsNullOrEmpty(_statusMessage))
                    DrawCenteredPixelString(_statusMessage, 580, 2, Color.Red);
            }
        }
        else if (_currentState == GameState.Victory || _currentState == GameState.GameOver)
        {
            Color bannerCol = _currentState == GameState.Victory ? Color.LimeGreen : Color.Crimson;
            string title = _currentState == GameState.Victory ? _currentVictoryTitle : _currentGameOverTitle;
            string sub = _currentState == GameState.Victory ? _currentVictorySubtext : _currentGameOverSubtext;

            DrawCenteredPixelString(title, 140, 3, bannerCol);
            DrawCenteredPixelString(sub, 220, 2, Color.White);

            DrawButton(_nextGameButton, Color.DarkBlue, _currentState == GameState.Victory ? "SUBJECT YOURSELF TO MORE TORTURE" : "EMBARRASS YOURSELF AGAIN");
            DrawButton(_victoryMenuButton, Color.Maroon, "RUN AWAY TO MENU LIKE A COWARD");
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

    private void DrawButton(Rectangle rect, Color color, string label)
    {
        DrawHollowRect(new Rectangle(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6), 3, Color.White);
        _spriteBatch.Draw(_pixel, rect, color);

        int scale = label.Length > 25 ? 1 : 2;
        DrawCenteredPixelString(label, rect.Y + (rect.Height - (7 * scale)) / 2, scale, Color.White);
    }

    private void DrawCenteredPixelString(string text, int y, int scale, Color color)
    {
        int textWidth = text.Length * (6 * scale);
        int x = (TargetWidth - textWidth) / 2;
        DrawPixelString(text, x, y, scale, color);
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