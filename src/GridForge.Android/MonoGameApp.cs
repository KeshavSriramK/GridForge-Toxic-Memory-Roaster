using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    private SpriteFont? _font;

    private const int VirtualWidth = 800;
    private const int VirtualHeight = 700;
    private const int GridSize = 20;
    private const int CellSize = 25;
    private const int OffsetX = 150;
    private const int OffsetY = 40;

    private GameState _currentState = GameState.NameInput;
    private GameMode _currentMode = GameMode.None;

    private string _userName = "Genius";
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

    // UI Rectangles
    private Rectangle _btnEasy = new(150, 260, 500, 65);
    private Rectangle _btnNormal = new(150, 345, 500, 65);
    private Rectangle _btnHard = new(150, 430, 500, 65);
    private Rectangle _confirmNameBtn = new(250, 400, 300, 50);
    private Rectangle _startTutorialBtn = new(200, 580, 400, 55);
    private Rectangle _startPlayingButton = new(200, 625, 400, 45);
    private Rectangle _checkButton = new(140, 620, 150, 40);
    private Rectangle _resetButton = new(325, 620, 150, 40);
    private Rectangle _menuButton = new(510, 620, 150, 40);
    private Rectangle _nextGameButton = new(150, 400, 500, 50);
    private Rectangle _victoryMenuButton = new(150, 470, 500, 50);

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

        base.Initialize();
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
            Vector2 pos = touch.Position;

            if (touch.State == TouchLocationState.Pressed)
            {
                if (_currentState == GameState.NameInput)
                {
                    _currentState = GameState.MainMenu;
                }
                else if (_currentState == GameState.MainMenu)
                {
                    if (_btnEasy.Contains(pos)) StartMode(GameMode.Easy);
                    else if (_btnNormal.Contains(pos)) StartMode(GameMode.Normal);
                    else if (_btnHard.Contains(pos)) StartMode(GameMode.Hard);
                }
                else if (_currentState == GameState.Tutorial && _startTutorialBtn.Contains(pos))
                {
                    LoadGameStage(_currentLevel, _currentGame);
                }
                else if (_currentState == GameState.Memorizing && _startPlayingButton.Contains(pos))
                {
                    _currentState = GameState.Playing;
                }
                else if (_currentState == GameState.Playing)
                {
                    if (_checkButton.Contains(pos))
                    {
                        if (ValidateExactUserPath()) _currentState = GameState.Victory;
                        else { _statusMessage = "WRONG PATH! TRY AGAIN!"; _statusMessageTimer = 3.0f; }
                    }
                    else if (_resetButton.Contains(pos)) _userDrawnPath.Clear();
                    else if (_menuButton.Contains(pos)) _currentState = GameState.MainMenu;
                }
                else if ((_currentState == GameState.Victory || _currentState == GameState.GameOver) && _nextGameButton.Contains(pos))
                {
                    int nextGame = _currentGame + 1;
                    int nextLevel = _currentLevel;
                    if (nextGame > GamesPerLevel) { nextGame = 1; nextLevel++; if (nextLevel > _maxLevels) nextLevel = 1; }
                    LoadGameStage(nextLevel, nextGame);
                }
            }

            if (_currentState == GameState.Playing && (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved))
            {
                int gx = (int)((pos.X - OffsetX) / CellSize);
                int gy = (int)((pos.Y - OffsetY) / CellSize);
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
        GraphicsDevice.Clear(new Color(15, 23, 42));

        _spriteBatch.Begin();

        if (_currentState == GameState.Memorizing || _currentState == GameState.Playing)
        {
            // Grid Rendering
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    var node = _grid[x, y];
                    Color col = node.IsWalkable ? Color.LightGray : Color.DarkGray;

                    if (_currentState == GameState.Memorizing && _targetSolutionPath.Contains(node))
                        col = Color.SkyBlue;
                    else if (_currentState == GameState.Playing && _userDrawnPath.Contains((x, y)))
                        col = Color.SkyBlue;

                    _spriteBatch.Draw(_pixel, new Rectangle(OffsetX + x * CellSize, OffsetY + y * CellSize, CellSize - 2, CellSize - 2), col);
                }
            }

            // Start & End Cells
            _spriteBatch.Draw(_pixel, new Rectangle(OffsetX + 2 * CellSize, OffsetY + 10 * CellSize, CellSize - 2, CellSize - 2), Color.Green);
            _spriteBatch.Draw(_pixel, new Rectangle(OffsetX + 18 * CellSize, OffsetY + 10 * CellSize, CellSize - 2, CellSize - 2), Color.Red);

            // Buttons
            if (_currentState == GameState.Memorizing)
                _spriteBatch.Draw(_pixel, _startPlayingButton, Color.DarkGreen);
            else
            {
                _spriteBatch.Draw(_pixel, _checkButton, Color.DarkGreen);
                _spriteBatch.Draw(_pixel, _resetButton, Color.DarkBlue);
                _spriteBatch.Draw(_pixel, _menuButton, Color.Maroon);
            }
        }
        else if (_currentState == GameState.MainMenu)
        {
            _spriteBatch.Draw(_pixel, _btnEasy, Color.Green);
            _spriteBatch.Draw(_pixel, _btnNormal, Color.Gold);
            _spriteBatch.Draw(_pixel, _btnHard, Color.Crimson);
        }
        else if (_currentState == GameState.NameInput)
        {
            _spriteBatch.Draw(_pixel, _confirmNameBtn, Color.LimeGreen);
        }
        else if (_currentState == GameState.Tutorial)
        {
            _spriteBatch.Draw(_pixel, _startTutorialBtn, Color.DarkGreen);
        }
        else if (_currentState == GameState.Victory || _currentState == GameState.GameOver)
        {
            _spriteBatch.Draw(_pixel, _nextGameButton, Color.DarkBlue);
            _spriteBatch.Draw(_pixel, _victoryMenuButton, Color.Maroon);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}