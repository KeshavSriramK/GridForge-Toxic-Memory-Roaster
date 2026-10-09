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

    private const int TargetWidth = 800;
    private const int TargetHeight = 700;
    private const int GridSize = 20;
    private const int CellSize = 25; // 20 * 25 = 500px grid

    // Centered positions inside 800x700 virtual space
    private const int GridOffsetX = 150; 
    private const int GridOffsetY = 100;

    private float _scale = 1f;
    private Vector2 _screenOffset = Vector2.Zero;

    private GameState _currentState = GameState.MainMenu;
    private GameMode _currentMode = GameMode.None;

    private string _userName = "GENIUS";
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

    private readonly Random _random = new();

    // Virtual UI Rectangles (Centered Layout)
    private readonly Rectangle _btnEasy = new(200, 220, 400, 60);
    private readonly Rectangle _btnNormal = new(200, 310, 400, 60);
    private readonly Rectangle _btnHard = new(200, 400, 400, 60);

    private readonly Rectangle _startTutorialBtn = new(200, 580, 400, 55);
    private readonly Rectangle _startPlayingButton = new(200, 620, 400, 50);

    private readonly Rectangle _checkButton = new(150, 620, 140, 50);
    private readonly Rectangle _resetButton = new(330, 620, 140, 50);
    private readonly Rectangle _menuButton = new(510, 620, 140, 50);

    private readonly Rectangle _nextGameButton = new(200, 450, 400, 55);
    private readonly Rectangle _victoryMenuButton = new(200, 520, 400, 55);

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
                        else { _statusMessage = "WRONG PATH! ROASTED!"; _statusMessageTimer = 3.0f; }
                    }
                    else if (_resetButton.Contains(posPoint)) _userDrawnPath.Clear();
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
                        _userDrawnPath.Add((gx, gy));
                }
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // Transform Matrix for automatic screen centering & scaling on any device
        Matrix transform = Matrix.CreateScale(_scale) * Matrix.CreateTranslation(_screenOffset.X, _screenOffset.Y, 0);

        _spriteBatch.Begin(transformMatrix: transform);

        // Virtual Canvas Background
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, TargetWidth, TargetHeight), new Color(15, 23, 42));

        if (_currentState == GameState.MainMenu)
        {
            DrawHeader("MEMORY ROASTER 3000", new Color(56, 189, 248));
            DrawButton(_btnEasy, Color.Green, "EASY MODE");
            DrawButton(_btnNormal, Color.Gold, "NORMAL MODE");
            DrawButton(_btnHard, Color.Crimson, "HARD MODE");
        }
        else if (_currentState == GameState.Tutorial)
        {
            DrawHeader("HOW TO PLAY", new Color(56, 189, 248));
            DrawButton(_startTutorialBtn, Color.DarkGreen, "START GAME");
        }
        else if (_currentState == GameState.Memorizing || _currentState == GameState.Playing)
        {
            // Timer Bar
            if (_currentState == GameState.Playing)
            {
                float pct = _timeRemaining / _maxTimeForLevel;
                Color timerCol = pct > 0.4f ? Color.LimeGreen : Color.Red;
                _spriteBatch.Draw(_pixel, new Rectangle(GridOffsetX, 40, (int)(500 * pct), 15), timerCol);
            }

            // Grid Rendering (Centered)
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    var node = _grid[x, y];
                    Color cellColor = node.IsWalkable ? new Color(226, 232, 240) : new Color(71, 85, 105);

                    if (_currentState == GameState.Memorizing && _targetSolutionPath.Contains(node))
                        cellColor = new Color(56, 189, 248); // Solution path
                    else if (_currentState == GameState.Playing && _userDrawnPath.Contains((x, y)))
                        cellColor = new Color(56, 189, 248); // Player path

                    _spriteBatch.Draw(_pixel, new Rectangle(GridOffsetX + x * CellSize, GridOffsetY + y * CellSize, CellSize - 2, CellSize - 2), cellColor);
                }
            }

            // Start (Green) & End (Red)
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
                DrawButton(_resetButton, Color.DarkBlue, "RESET");
                DrawButton(_menuButton, Color.Maroon, "MENU");
            }
        }
        else if (_currentState == GameState.Victory || _currentState == GameState.GameOver)
        {
            Color bannerCol = _currentState == GameState.Victory ? Color.LimeGreen : Color.Crimson;
            string title = _currentState == GameState.Victory ? "LEVEL CLEARED!" : "SYSTEM OVERLOAD!";
            DrawHeader(title, bannerCol);

            DrawButton(_nextGameButton, Color.DarkBlue, "NEXT LEVEL");
            DrawButton(_victoryMenuButton, Color.Maroon, "MAIN MENU");
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawHeader(string title, Color color)
    {
        _spriteBatch.Draw(_pixel, new Rectangle(100, 50, 600, 80), color);
    }

    private void DrawButton(Rectangle rect, Color color, string label)
    {
        // Border + Fill
        _spriteBatch.Draw(_pixel, new Rectangle(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6), Color.White);
        _spriteBatch.Draw(_pixel, rect, color);
    }
}