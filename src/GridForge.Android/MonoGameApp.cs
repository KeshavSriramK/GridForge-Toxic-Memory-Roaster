using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using GridForge.Core.AI.Pathfinding;

namespace GridForge.Android;

public enum GameMode { None, Easy, Normal, Hard }
public enum GameState { MainMenu, Tutorial, Memorizing, Playing, Victory, GameOver }

public class MonoGameApp : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;

    private const int GridSize = 20;
    private int _cellSize = 20;
    private int _gridOffsetX = 0;
    private int _gridOffsetY = 0;

    private GameState _currentState = GameState.MainMenu;
    private GameMode _currentMode = GameMode.None;

    private string _userName = "GORI";
    private int _currentLevel = 1;
    private int _currentGame = 1;
    private int _maxLevels = 5;
    private const int GamesPerLevel = 5;

    private float _timeRemaining = 30f;
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
    private string _currentGameOverHeadline = "";
    private string _currentGameOverSubtext = "";

    private readonly Random _random = new();

    // Responsive Button Rectangles
    private Rectangle _btnEasy;
    private Rectangle _btnNormal;
    private Rectangle _btnHard;
    private Rectangle _startTutorialBtn;
    private Rectangle _startPlayingButton;
    private Rectangle _checkButton;
    private Rectangle _resetButton;
    private Rectangle _menuButton;
    private Rectangle _nextGameButton;
    private Rectangle _victoryMenuButton;

    // 5x7 Pixel Font Map
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
        { '.', new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x0C } },
        { ',', new byte[] { 0x00, 0x00, 0x00, 0x00, 0x0C, 0x04, 0x08 } },
        { '(', new byte[] { 0x02, 0x04, 0x08, 0x08, 0x08, 0x04, 0x02 } },
        { ')', new byte[] { 0x08, 0x04, 0x02, 0x02, 0x02, 0x04, 0x08 } },
        { '/', new byte[] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x00, 0x00 } },
        { '-', new byte[] { 0x00, 0x00, 0x00, 0x1F, 0x00, 0x00, 0x00 } }
    };

    public MonoGameApp()
    {
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

        RecalculateLayout();
        base.Initialize();
    }

    private void RecalculateLayout()
    {
        int screenW = GraphicsDevice.Viewport.Width;
        int screenH = GraphicsDevice.Viewport.Height;

        bool isLandscape = screenW > screenH;

        if (isLandscape)
        {
            int maxGridHeight = screenH - 120;
            _cellSize = Math.Max(12, maxGridHeight / GridSize);
            int totalGridDim = _cellSize * GridSize;

            _gridOffsetX = (screenW - totalGridDim) / 2;
            _gridOffsetY = 60;

            int btnWidth = 140;
            int btnHeight = 45;
            int btnY = screenH - 55;

            _checkButton = new Rectangle(_gridOffsetX, btnY, btnWidth, btnHeight);
            _resetButton = new Rectangle(_gridOffsetX + (totalGridDim / 2) - (btnWidth / 2), btnY, btnWidth, btnHeight);
            _menuButton = new Rectangle(_gridOffsetX + totalGridDim - btnWidth, btnY, btnWidth, btnHeight);
        }
        else
        {
            // Portrait Mobile Layout
            int maxGridWidth = screenW - 40;
            _cellSize = Math.Max(10, maxGridWidth / GridSize);
            int totalGridDim = _cellSize * GridSize;

            _gridOffsetX = (screenW - totalGridDim) / 2;
            _gridOffsetY = 80;

            int btnWidth = (totalGridDim - 20) / 3;
            int btnHeight = 50;
            int btnY = _gridOffsetY + totalGridDim + 20;

            _checkButton = new Rectangle(_gridOffsetX, btnY, btnWidth, btnHeight);
            _resetButton = new Rectangle(_gridOffsetX + btnWidth + 10, btnY, btnWidth, btnHeight);
            _menuButton = new Rectangle(_gridOffsetX + (btnWidth + 10) * 2, btnY, btnWidth, btnHeight);
        }

        int menuWidth = Math.Min(screenW - 60, 450);
        int menuX = (screenW - menuWidth) / 2;

        _btnEasy = new Rectangle(menuX, 180, menuWidth, 55);
        _btnNormal = new Rectangle(menuX, 250, menuWidth, 55);
        _btnHard = new Rectangle(menuX, 320, menuWidth, 55);

        _startTutorialBtn = new Rectangle(menuX, screenH - 80, menuWidth, 50);
        _startPlayingButton = new Rectangle(menuX, screenH - 80, menuWidth, 50);

        _nextGameButton = new Rectangle(menuX, screenH - 140, menuWidth, 50);
        _victoryMenuButton = new Rectangle(menuX, screenH - 75, menuWidth, 50);
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

    private void TriggerVictory()
    {
        string name = _userName.ToUpper();
        string[] headlines = {
            $"OH, YOU ACTUALLY DID IT, {name}?",
            $"PURE BLIND LUCK, {name}!",
            $"WHO LET {name} WIN?",
            $"CONGRATULATIONS, {name} THE GOLDFISH!"
        };

        string[] subtexts = {
            $"{name}, you managed to trace a line without drooling.",
            $"Even a toddler could do it, but hey, take your win.",
            $"Don't let it go to your head {name}, your IQ is still in danger.",
            $"Miracles happen. Too bad the next stage will crush you."
        };

        _currentVictoryHeadline = headlines[_random.Next(headlines.Length)];
        _currentVictorySubtext = subtexts[_random.Next(subtexts.Length)];
        _currentState = GameState.Victory;
    }

    private void TriggerGameOver()
    {
        string name = _userName.ToUpper();
        string[] headlines = {
            $"TIME'S UP, {name}.",
            $"{name} HAS THE MEMORY OF A GOLDFISH.",
            $"ABSOLUTE DISASTER, {name}!",
            $"YIKES, {name}. JUST YIKES."
        };

        string[] subtexts = {
            $"That path was on screen for seconds. Did you forget how to see?",
            $"Your brain cells fired, but unfortunately none connected.",
            $"Even a goldfish remembers things longer than {name}.",
            $"That was painful to watch, {name}. Try using your brain."
        };

        _currentGameOverHeadline = headlines[_random.Next(headlines.Length)];
        _currentGameOverSubtext = subtexts[_random.Next(subtexts.Length)];
        _currentState = GameState.GameOver;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_statusMessageTimer > 0)
        {
            _statusMessageTimer -= dt;
            if (_statusMessageTimer <= 0) _statusMessage = "";
        }

        if (_currentState == GameState.Playing)
        {
            _timeRemaining -= dt;
            if (_timeRemaining <= 0)
            {
                _timeRemaining = 0;
                TriggerGameOver();
                return;
            }
        }

        var touchState = TouchPanel.GetState();
        if (touchState.Count > 0)
        {
            var touch = touchState[0];
            Vector2 touchPos = touch.Position;
            Point posPoint = new((int)touchPos.X, (int)touchPos.Y);

            if (touch.State == TouchLocationState.Pressed)
            {
                if (_currentState == GameState.MainMenu)
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
                            string[] insults = {
                                $"Are your eyes painted on, {_userName}? Totally wrong.",
                                $"Did you draw this with your eyes closed?",
                                $"Absolute trash tier drawing, {_userName}.",
                                $"Epic fail. That path looks like a toddler's scribble."
                            };
                            _statusMessage = insults[_random.Next(insults.Length)];
                            _statusMessageTimer = 3.0f;
                        }
                    }
                    else if (_resetButton.Contains(posPoint))
                    {
                        _userDrawnPath.Clear();
                    }
                    else if (_menuButton.Contains(posPoint))
                    {
                        _currentState = GameState.MainMenu;
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
                int gx = (int)((touchPos.X - _gridOffsetX) / _cellSize);
                int gy = (int)((touchPos.Y - _gridOffsetY) / _cellSize);
                if (gx >= 0 && gx < GridSize && gy >= 0 && gy < GridSize)
                {
                    if (_grid[gx, gy].IsWalkable && !((gx == 2 && gy == 10) || (gx == 18 && gy == 10)))
                    {
                        var cell = (gx, gy);
                        if (!_userDrawnPath.Contains(cell))
                            _userDrawnPath.Add(cell);
                    }
                }
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();

        int screenW = GraphicsDevice.Viewport.Width;
        int screenH = GraphicsDevice.Viewport.Height;

        if (_currentState == GameState.MainMenu)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, screenW, screenH), new Color(15, 23, 42));

            DrawPixelString("MEMORY ROASTER 3000", (screenW - 380) / 2, 60, 3, Color.SkyBlue);
            DrawPixelString($"Player: {_userName}", (screenW - 180) / 2, 110, 2, Color.Gold);

            DrawButton(_btnEasy, new Color(21, 128, 61), "EASY (3 Brain Cells)");
            DrawButton(_btnNormal, new Color(202, 138, 4), "NORMAL (You'll Fail)");
            DrawButton(_btnHard, new Color(190, 18, 60), "HARD (Prepare To Cry)");
        }
        else if (_currentState == GameState.Tutorial)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, screenW, screenH), Color.WhiteSmoke);

            DrawPixelString($"WELCOME, {_userName.ToUpper()}!", 30, 30, 3, Color.DarkBlue);
            DrawPixelString("HOW TO NOT EMBARRASS YOURSELF", 30, 70, 2, Color.Black);

            DrawPixelString("1. Look at the blue line. Try to actually use your brain.", 30, 130, 2, Color.Black);
            DrawPixelString("2. Click start to hide it. Try not to panic immediately.", 30, 175, 2, Color.Black);
            DrawPixelString("3. Touch & drag across cells to redraw what you forgot.", 30, 220, 2, Color.Black);
            DrawPixelString("4. Tap Wipe Shame to erase your miserable mistakes.", 30, 265, 2, Color.DarkBlue);
            DrawPixelString("5. If you fail, we will judge you loudly.", 30, 310, 2, Color.Maroon);

            DrawButton(_startTutorialBtn, Color.DarkGreen, "I DARE TO TRY");
        }
        else if (_currentState == GameState.Memorizing || _currentState == GameState.Playing)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, screenW, screenH), Color.WhiteSmoke);

            DrawPixelString($"Player: {_userName} | {_currentMode} Lvl {_currentLevel}/{_maxLevels}", _gridOffsetX, 15, 2, Color.DarkGray);

            if (_currentState == GameState.Playing)
            {
                float timerPct = _timeRemaining / _maxTimeForLevel;
                Color timerColor = timerPct > 0.4f ? Color.DarkGreen : Color.Maroon;
                _spriteBatch.Draw(_pixel, new Rectangle(_gridOffsetX, 38, (int)((_cellSize * GridSize) * timerPct), 8), timerColor);
            }

            // Grid Rendering (Fluid Screen Size)
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    var node = _grid[x, y];
                    Color color = node.IsWalkable ? Color.LightGray : Color.DarkGray;

                    if (_currentState == GameState.Memorizing && _targetSolutionPath.Contains(node))
                        color = Color.SkyBlue;
                    else if (_currentState == GameState.Playing && _userDrawnPath.Contains((x, y)))
                        color = Color.SkyBlue;

                    _spriteBatch.Draw(_pixel, new Rectangle(_gridOffsetX + x * _cellSize, _gridOffsetY + y * _cellSize, _cellSize - 2, _cellSize - 2), color);
                }
            }

            // Green Start & Red End
            _spriteBatch.Draw(_pixel, new Rectangle(_gridOffsetX + 2 * _cellSize, _gridOffsetY + 10 * _cellSize, _cellSize - 2, _cellSize - 2), Color.LimeGreen);
            _spriteBatch.Draw(_pixel, new Rectangle(_gridOffsetX + 18 * _cellSize, _gridOffsetY + 10 * _cellSize, _cellSize - 2, _cellSize - 2), Color.Red);

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                DrawPixelString(_statusMessage, _gridOffsetX, _gridOffsetY - 20, 2, Color.Maroon);
            }

            if (_currentState == GameState.Memorizing)
            {
                DrawButton(_startPlayingButton, Color.DarkGreen, "I'M READY (PROVE ME WRONG)");
            }
            else
            {
                DrawButton(_checkButton, Color.DarkGreen, "CHECK WORK");
                DrawButton(_resetButton, Color.DarkBlue, "Wipe Shame");
                DrawButton(_menuButton, Color.Maroon, "Give Up");
            }
        }
        else if (_currentState == GameState.Victory || _currentState == GameState.GameOver)
        {
            _spriteBatch.Draw(_pixel, new Rectangle(0, 0, screenW, screenH), Color.WhiteSmoke);

            Color borderCol = _currentState == GameState.Victory ? Color.DarkGreen : Color.Maroon;
            DrawPixelString(_currentVictoryHeadline, 30, 100, 2, borderCol);
            DrawPixelString(_currentVictorySubtext, 30, 160, 2, Color.DarkGray);

            DrawButton(_nextGameButton, Color.DarkBlue, "Embarrass Yourself Again");
            DrawButton(_victoryMenuButton, Color.Maroon, "Run Away to Main Menu");
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawButton(Rectangle rect, Color color, string label)
    {
        _spriteBatch.Draw(_pixel, new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 4), Color.White);
        _spriteBatch.Draw(_pixel, rect, color);

        int textWidth = label.Length * 12;
        int textX = rect.X + (rect.Width - textWidth) / 2;
        int textY = rect.Y + (rect.Height - 14) / 2;
        DrawPixelString(label, textX, textY, 2, Color.White);
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