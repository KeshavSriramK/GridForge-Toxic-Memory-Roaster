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

    private const int TargetWidth = 800;
    private const int TargetHeight = 700;
    private const int GridSize = 20;
    private const int CellSize = 25;

    private const int GridOffsetX = 150; 
    private const int GridOffsetY = 100;

    private float _scale = 1f;
    private Vector2 _screenOffset = Vector2.Zero;

    private GameState _currentState = GameState.MainMenu;
    private GameMode _currentMode = GameMode.None;

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
    private List<(int X, int Y)> _userDrawnPathList = new();
    private HashSet<(int X, int Y)> _userDrawnPathSet = new();

    private string _statusMessage = "";
    private float _statusMessageTimer = 0f;

    private readonly Random _random = new();

    // UI Rectangles
    private readonly Rectangle _btnEasy = new(200, 200, 400, 60);
    private readonly Rectangle _btnNormal = new(200, 290, 400, 60);
    private readonly Rectangle _btnHard = new(200, 380, 400, 60);

    private readonly Rectangle _startTutorialBtn = new(200, 580, 400, 55);
    private readonly Rectangle _startPlayingButton = new(200, 620, 400, 50);

    private readonly Rectangle _checkButton = new(80, 620, 140, 50);
    private readonly Rectangle _undoButton = new(240, 620, 140, 50);
    private readonly Rectangle _peekButton = new(400, 620, 140, 50);
    private readonly Rectangle _menuButton = new(560, 620, 140, 50);

    private readonly Rectangle _nextGameButton = new(200, 450, 400, 55);
    private readonly Rectangle _victoryMenuButton = new(200, 520, 400, 55);

    // Procedural 5x7 Pixel Font Mapping
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
        { ':', new byte[] { 0x00, 0x0C, 0x0C, 0x00, 0x0C, 0x0C, 0x00 } },
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
        _userDrawnPathList.Clear();
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
                _currentState = GameState.GameOver;
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
                        if (ValidateExactUserPath()) _currentState = GameState.Victory;
                        else { _statusMessage = "WRONG PATH! TRY AGAIN!"; _statusMessageTimer = 3.0f; }
                    }
                    else if (_undoButton.Contains(posPoint))
                    {
                        // Undo last drawn cell
                        if (_userDrawnPathList.Count > 0)
                        {
                            var last = _userDrawnPathList[^1];
                            _userDrawnPathList.RemoveAt(_userDrawnPathList.Count - 1);
                            _userDrawnPathSet.Remove(last);
                        }
                    }
                    else if (_peekButton.Contains(posPoint))
                    {
                        // Flash solution path for 2 seconds (5s penalty)
                        _peekTimer = 2.0f;
                        _timeRemaining = Math.Max(1f, _timeRemaining - 5f);
                    }
                    else if (_menuButton.Contains(posPoint)) _currentState = GameState.MainMenu;
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
                        if (!_userDrawnPathSet.Contains(cell))
                        {
                            _userDrawnPathSet.Add(cell);
                            _userDrawnPathList.Add(cell);
                        }
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

        if (_currentState == GameState.MainMenu)
        {
            DrawPixelString("MEMORY ROASTER 3000", 220, 80, 3, Color.SkyBlue);
            DrawButton(_btnEasy, Color.Green, "EASY MODE");
            DrawButton(_btnNormal, Color.Gold, "NORMAL MODE");
            DrawButton(_btnHard, Color.Crimson, "HARD MODE");
        }
        else if (_currentState == GameState.Tutorial)
        {
            DrawPixelString("MEMORIZE BLUE PATH", 200, 100, 3, Color.SkyBlue);
            DrawPixelString("DRAW IT FROM GREEN TO RED", 140, 180, 2, Color.White);
            DrawButton(_startTutorialBtn, Color.DarkGreen, "START GAME");
        }
        else if (_currentState == GameState.Memorizing || _currentState == GameState.Playing)
        {
            if (_currentState == GameState.Playing)
            {
                float pct = _timeRemaining / _maxTimeForLevel;
                Color timerCol = pct > 0.4f ? Color.LimeGreen : Color.Red;
                _spriteBatch.Draw(_pixel, new Rectangle(GridOffsetX, 40, (int)(500 * pct), 15), timerCol);
                DrawPixelString($"TIME: {(int)_timeRemaining}S", 680, 38, 2, Color.White);
            }

            // Grid Rendering
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

            // Start & End Cells
            _spriteBatch.Draw(_pixel, new Rectangle(GridOffsetX + 2 * CellSize, GridOffsetY + 10 * CellSize, CellSize - 2, CellSize - 2), Color.LimeGreen);
            _spriteBatch.Draw(_pixel, new Rectangle(GridOffsetX + 18 * CellSize, GridOffsetY + 10 * CellSize, CellSize - 2, CellSize - 2), Color.Red);

            // Action Buttons
            if (_currentState == GameState.Memorizing)
            {
                DrawButton(_startPlayingButton, Color.DarkGreen, "START DRAWING");
            }
            else
            {
                DrawButton(_checkButton, Color.DarkGreen, "CHECK");
                DrawButton(_undoButton, Color.DarkBlue, "UNDO");
                DrawButton(_peekButton, Color.DarkGoldenrod, "PEEK (-5S)");
                DrawButton(_menuButton, Color.Maroon, "MENU");

                if (!string.IsNullOrEmpty(_statusMessage))
                    DrawPixelString(_statusMessage, 260, 580, 2, Color.Red);
            }
        }
        else if (_currentState == GameState.Victory || _currentState == GameState.GameOver)
        {
            Color bannerCol = _currentState == GameState.Victory ? Color.LimeGreen : Color.Crimson;
            string title = _currentState == GameState.Victory ? "VICTORY!" : "GAME OVER!";
            DrawPixelString(title, 320, 200, 4, bannerCol);

            DrawButton(_nextGameButton, Color.DarkBlue, "NEXT LEVEL");
            DrawButton(_victoryMenuButton, Color.Maroon, "MAIN MENU");
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private void DrawButton(Rectangle rect, Color color, string label)
    {
        _spriteBatch.Draw(_pixel, new Rectangle(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6), Color.White);
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