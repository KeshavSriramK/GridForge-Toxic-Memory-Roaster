using Raylib_cs;
using GridForge.Core.Interfaces;
using GridForge.Core.AI.Pathfinding;
using System.Collections.Generic;
using System.Numerics;
using System;

namespace GridForge.Core.Engine
{
    public enum GameMode
    {
        None,
        Easy,
        Normal,
        Hard
    }

    public enum GameState
    {
        NameInput,
        MainMenu,
        Tutorial,
        Memorizing,
        Playing,
        Victory,
        GameOver
    }

    public class GameApp : IGameLoop
    {
        // Fixed Virtual Canvas Resolution
        private const int VirtualWidth = 800;
        private const int VirtualHeight = 700;

        private const int GridSize = 20;
        private const int CellSize = 25;
        private const int OffsetX = 150;
        private const int OffsetY = 40;

        private RenderTexture2D _targetCanvas;

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

        private struct Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Size;
            public Color Color;
        }
        private List<Particle> _backgroundParticles = new List<Particle>();

        // UI Buttons (Virtual Coordinates)
        private Rectangle _btnEasy = new Rectangle(150, 260, 500, 65);
        private Rectangle _btnNormal = new Rectangle(150, 345, 500, 65);
        private Rectangle _btnHard = new Rectangle(150, 430, 500, 65);

        private Rectangle _nameInputBox = new Rectangle(200, 320, 400, 50);
        private Rectangle _confirmNameBtn = new Rectangle(250, 400, 300, 50);

        private Rectangle _startTutorialBtn = new Rectangle(200, 580, 400, 55);
        private Rectangle _startPlayingButton = new Rectangle(200, 625, 400, 45);

        private Rectangle _checkButton = new Rectangle(140, 620, 150, 40);
        private Rectangle _resetButton = new Rectangle(325, 620, 150, 40);
        private Rectangle _menuButton = new Rectangle(510, 620, 150, 40);

        private Rectangle _nextGameButton = new Rectangle(150, 400, 500, 50);
        private Rectangle _victoryMenuButton = new Rectangle(150, 470, 500, 50);

        private Random _random = new Random();

        public void Initialize()
        {
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
            Raylib.InitWindow(VirtualWidth, VirtualHeight, "GridForge Engine - Toxic Memory Roaster");
            Raylib.SetTargetFPS(60);

            // Create off-screen virtual texture canvas
            _targetCanvas = Raylib.LoadRenderTexture(VirtualWidth, VirtualHeight);
            Raylib.SetTextureFilter(_targetCanvas.Texture, TextureFilter.Bilinear);

            for (int i = 0; i < 40; i++)
            {
                _backgroundParticles.Add(new Particle
                {
                    Position = new Vector2((float)_random.NextDouble() * VirtualWidth, (float)_random.NextDouble() * VirtualHeight),
                    Velocity = new Vector2((float)(_random.NextDouble() * 1.5 - 0.75), (float)(_random.NextDouble() * -1.5 - 0.5)),
                    Size = (float)(_random.NextDouble() * 4 + 2),
                    Color = new Color(
                        (byte)_random.Next(30, 90),
                        (byte)_random.Next(30, 90),
                        (byte)_random.Next(100, 200),
                        (byte)_random.Next(80, 180)
                    )
                });
            }
        }

        private Vector2 GetVirtualInputPosition()
        {
            Vector2 rawPosition = Raylib.GetMousePosition();
            if (Raylib.GetTouchPointCount() > 0)
            {
                rawPosition = Raylib.GetTouchPosition(0);
            }

            float scale = Math.Min(
                (float)Raylib.GetScreenWidth() / VirtualWidth,
                (float)Raylib.GetScreenHeight() / VirtualHeight
            );

            float offsetX = (Raylib.GetScreenWidth() - (VirtualWidth * scale)) * 0.5f;
            float offsetY = (Raylib.GetScreenHeight() - (VirtualHeight * scale)) * 0.5f;

            Vector2 virtualPos = new Vector2(
                (rawPosition.X - offsetX) / scale,
                (rawPosition.Y - offsetY) / scale
            );

            virtualPos.X = Math.Clamp(virtualPos.X, 0, VirtualWidth);
            virtualPos.Y = Math.Clamp(virtualPos.Y, 0, VirtualHeight);

            return virtualPos;
        }

        private bool IsPrimaryInputPressed()
        {
            return Raylib.IsMouseButtonPressed(MouseButton.Left) || (Raylib.GetTouchPointCount() > 0 && Raylib.IsGestureDetected(Gesture.Tap));
        }

        private bool IsPrimaryInputDown()
        {
            return Raylib.IsMouseButtonDown(MouseButton.Left) || Raylib.GetTouchPointCount() > 0;
        }

        private void StartMode(GameMode mode)
        {
            _currentMode = mode;
            _currentLevel = 1;
            _currentGame = 1;

            if (mode == GameMode.Easy)
            {
                _maxLevels = 5;
                _maxTimeForLevel = 30f;
            }
            else if (mode == GameMode.Normal)
            {
                _maxLevels = 10;
                _maxTimeForLevel = 20f;
            }
            else if (mode == GameMode.Hard)
            {
                _maxLevels = 20;
                _maxTimeForLevel = 15f;
            }

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
            {
                for (int y = 0; y < GridSize; y++)
                {
                    _grid[x, y] = new PathNode(x, y, true);
                }
            }

            int globalGameIndex = ((level - 1) * GamesPerLevel) + game;
            int seed = ((int)_currentMode * 10000) + (level * 100) + game;
            Random rand = new Random(seed);

            double obstacleChance = 0.08;
            if (_currentMode == GameMode.Easy) obstacleChance = 0.08 + (globalGameIndex * 0.005);
            else if (_currentMode == GameMode.Normal) obstacleChance = 0.14 + (globalGameIndex * 0.004);
            else if (_currentMode == GameMode.Hard) obstacleChance = 0.22 + (globalGameIndex * 0.002);

            for (int x = 3; x < 17; x++)
            {
                for (int y = 2; y < 18; y++)
                {
                    if ((x == 2 && y == 10) || (x == 18 && y == 10)) continue;

                    if (rand.NextDouble() < obstacleChance)
                    {
                        _grid[x, y].IsWalkable = false;
                    }
                }
            }

            _pathfinder = new AStarPathfinder(_grid);
            _targetSolutionPath = _pathfinder.FindPath(2, 10, 18, 10);

            while (_targetSolutionPath == null || _targetSolutionPath.Count == 0)
            {
                for (int x = 0; x < GridSize; x++)
                    for (int y = 0; y < GridSize; y++)
                        _grid[x, y].IsWalkable = true;

                for (int x = 3; x < 17; x++)
                {
                    for (int y = 2; y < 18; y++)
                    {
                        if ((x == 2 && y == 10) || (x == 18 && y == 10)) continue;
                        if (rand.NextDouble() < (obstacleChance * 0.7))
                        {
                            _grid[x, y].IsWalkable = false;
                        }
                    }
                }
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
            if (_userDrawnPath.Count != _targetSolutionSet.Count)
                return false;

            foreach (var cell in _userDrawnPath)
            {
                if (!_targetSolutionSet.Contains(cell))
                    return false;
            }

            return true;
        }

        private string GetActivePlayerName()
        {
            return string.IsNullOrWhiteSpace(_userName) ? "Genius" : _userName;
        }

        private void TriggerVictory()
        {
            string name = GetActivePlayerName();

            string[] headlines = new[]
            {
                $"OH, YOU ACTUALLY DID IT, {name.ToUpper()}?",
                $"PURE BLIND LUCK, {name.ToUpper()}!",
                $"WHO LET {name.ToUpper()} WIN?",
                $"CONGRATULATIONS, {name.ToUpper()} THE GOLDFISH!"
            };

            string[] subtexts = new[]
            {
                $"{name}, you managed to trace a line without drooling. Truly inspiring.",
                $"Even a toddler could do it, but hey, let's throw {name} a parade.",
                $"Don't let it go to your head {name}, your IQ is still in danger.",
                $"Miracles do happen. Too bad the next level will crush {name}'s fragile ego."
            };

            string[] btnTexts = new[]
            {
                "Subject Yourself to More",
                "Prove It Wasn't A Fluke",
                "Next Level of Torture"
            };

            _currentVictoryHeadline = headlines[_random.Next(headlines.Length)];
            _currentVictorySubtext = subtexts[_random.Next(subtexts.Length)];
            _currentVictoryBtnText = btnTexts[_random.Next(btnTexts.Length)];
            _currentState = GameState.Victory;
        }

        private void TriggerGameOver()
        {
            string name = GetActivePlayerName();

            string[] headlines = new[]
            {
                $"TIME'S UP, {name.ToUpper()}.",
                $"{name.ToUpper()} HAS THE MEMORY OF A GOLDFISH.",
                $"ABSOLUTE DISASTER, {name.ToUpper()}!",
                $"YIKES, {name.ToUpper()}. JUST YIKES."
            };

            string[] subtexts = new[]
            {
                $"That path was on screen for seconds. Did {name} forget how to see?",
                $"{name}'s brain cells fired, but unfortunately none of them connected.",
                $"Even a goldfish remembers things longer than {name}. Want a bib?",
                $"That was painful to watch, {name}. Try using your brain next time."
            };

            _currentGameOverHeadline = headlines[_random.Next(headlines.Length)];
            _currentGameOverSubtext = subtexts[_random.Next(subtexts.Length)];
            _currentState = GameState.GameOver;
        }

        public void Update(double deltaTime)
        {
            float dt = (float)deltaTime;

            if (_currentState == GameState.MainMenu || _currentState == GameState.NameInput)
            {
                for (int i = 0; i < _backgroundParticles.Count; i++)
                {
                    var p = _backgroundParticles[i];
                    p.Position += p.Velocity;
                    if (p.Position.Y < -10) p.Position.Y = VirtualHeight + 10;
                    if (p.Position.X < -10) p.Position.X = VirtualWidth + 10;
                    if (p.Position.X > VirtualWidth + 10) p.Position.X = -10;
                    _backgroundParticles[i] = p;
                }
            }

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

            Vector2 inputPos = GetVirtualInputPosition();
            bool inputPressed = IsPrimaryInputPressed();
            bool inputDown = IsPrimaryInputDown();

            if (_currentState == GameState.NameInput)
            {
                int key = Raylib.GetCharPressed();
                while (key > 0)
                {
                    if ((key >= 32) && (key <= 125) && (_nameInputBuffer.Length < MaxNameLength))
                    {
                        _nameInputBuffer += (char)key;
                    }
                    key = Raylib.GetCharPressed();
                }

                if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && _nameInputBuffer.Length > 0)
                {
                    _nameInputBuffer = _nameInputBuffer.Substring(0, _nameInputBuffer.Length - 1);
                }

                if (Raylib.IsKeyPressed(KeyboardKey.Enter) || (inputPressed && IsHovered(inputPos, _confirmNameBtn)))
                {
                    _userName = string.IsNullOrWhiteSpace(_nameInputBuffer) ? "Genius" : _nameInputBuffer;
                    _currentState = GameState.MainMenu;
                }
            }
            else if (_currentState == GameState.MainMenu)
            {
                if (inputPressed)
                {
                    if (IsHovered(inputPos, _btnEasy)) StartMode(GameMode.Easy);
                    else if (IsHovered(inputPos, _btnNormal)) StartMode(GameMode.Normal);
                    else if (IsHovered(inputPos, _btnHard)) StartMode(GameMode.Hard);
                }
            }
            else if (_currentState == GameState.Tutorial)
            {
                if (inputPressed && IsHovered(inputPos, _startTutorialBtn))
                {
                    LoadGameStage(_currentLevel, _currentGame);
                }
            }
            else if (_currentState == GameState.Memorizing)
            {
                if (inputPressed && IsHovered(inputPos, _startPlayingButton))
                {
                    _currentState = GameState.Playing;
                }
            }
            else if (_currentState == GameState.Playing)
            {
                if (inputPressed)
                {
                    if (IsHovered(inputPos, _checkButton))
                    {
                        if (ValidateExactUserPath())
                        {
                            TriggerVictory();
                        }
                        else
                        {
                            string name = GetActivePlayerName();
                            string[] insults = {
                                $"Are your eyes painted on, {name}? That's completely wrong.",
                                $"Not even close, {name}. Did you draw this blindfolded?",
                                $"Absolute trash tier drawing, {name}.",
                                $"Epic fail, {name}. That path looks like a toddler's scribble."
                            };
                            _statusMessage = insults[_random.Next(insults.Length)];
                            _statusMessageTimer = 3.0f;
                        }
                    }
                    else if (IsHovered(inputPos, _resetButton))
                    {
                        _userDrawnPath.Clear();
                    }
                    else if (IsHovered(inputPos, _menuButton))
                    {
                        _currentState = GameState.MainMenu;
                    }
                }

                int gridX = (int)((inputPos.X - OffsetX) / CellSize);
                int gridY = (int)((inputPos.Y - OffsetY) / CellSize);

                if (gridX >= 0 && gridX < GridSize && gridY >= 0 && gridY < GridSize)
                {
                    if (_grid[gridX, gridY].IsWalkable && !((gridX == 2 && gridY == 10) || (gridX == 18 && gridY == 10)))
                    {
                        if (inputDown)
                        {
                            _userDrawnPath.Add((gridX, gridY));
                        }
                        else if (Raylib.IsMouseButtonDown(MouseButton.Right))
                        {
                            _userDrawnPath.Remove((gridX, gridY));
                        }
                    }
                }
            }
            else if (_currentState == GameState.Victory || _currentState == GameState.GameOver)
            {
                if (inputPressed)
                {
                    if (IsHovered(inputPos, _nextGameButton))
                    {
                        int nextGame = _currentGame + 1;
                        int nextLevel = _currentLevel;

                        if (nextGame > GamesPerLevel)
                        {
                            nextGame = 1;
                            nextLevel++;
                            if (nextLevel > _maxLevels) nextLevel = 1;
                        }

                        LoadGameStage(nextLevel, nextGame);
                    }
                    else if (IsHovered(inputPos, _victoryMenuButton))
                    {
                        _currentState = GameState.MainMenu;
                    }
                }
            }
        }

        private bool IsHovered(Vector2 pos, Rectangle rect)
        {
            return pos.X >= rect.X && pos.X <= rect.X + rect.Width &&
                   pos.Y >= rect.Y && pos.Y <= rect.Y + rect.Height;
        }

        public void Render()
        {
            // 1. Draw to internal Virtual Texture Canvas
            Raylib.BeginTextureMode(_targetCanvas);
            Raylib.ClearBackground(Color.RayWhite);

            Vector2 inputPos = GetVirtualInputPosition();

            if (_currentState == GameState.NameInput)
            {
                Raylib.DrawRectangleGradientV(0, 0, VirtualWidth, VirtualHeight, new Color(15, 23, 42, 255), new Color(30, 41, 59, 255));
                foreach (var p in _backgroundParticles)
                {
                    Raylib.DrawCircleV(p.Position, p.Size, p.Color);
                }

                Raylib.DrawRectangle(100, 100, 600, 480, new Color(15, 23, 42, 220));
                Raylib.DrawRectangleLines(100, 100, 600, 480, Color.SkyBlue);

                string promptStr = "WHO IS DARING TO PLAY?";
                Raylib.DrawText(promptStr, (VirtualWidth - Raylib.MeasureText(promptStr, 28)) / 2, 160, 28, Color.SkyBlue);

                string subPromptStr = "Enter your name for this gaming session:";
                Raylib.DrawText(subPromptStr, (VirtualWidth - Raylib.MeasureText(subPromptStr, 18)) / 2, 220, 18, Color.LightGray);

                Raylib.DrawRectangle((int)_nameInputBox.X, (int)_nameInputBox.Y, (int)_nameInputBox.Width, (int)_nameInputBox.Height, Color.DarkBlue);
                Raylib.DrawRectangleLines((int)_nameInputBox.X, (int)_nameInputBox.Y, (int)_nameInputBox.Width, (int)_nameInputBox.Height, Color.SkyBlue);

                string displayName = string.IsNullOrEmpty(_nameInputBuffer) ? "Type name..." : _nameInputBuffer;
                Color nameColor = string.IsNullOrEmpty(_nameInputBuffer) ? Color.Gray : Color.White;
                Raylib.DrawText(displayName, (int)_nameInputBox.X + 20, (int)_nameInputBox.Y + 15, 20, nameColor);

                bool hoverConfirm = IsHovered(inputPos, _confirmNameBtn);
                Color btnCol = hoverConfirm ? Color.Lime : Color.Green;
                Raylib.DrawRectangle((int)_confirmNameBtn.X, (int)_confirmNameBtn.Y, (int)_confirmNameBtn.Width, (int)_confirmNameBtn.Height, btnCol);
                string confirmStr = "START SESSION (ENTER)";
                Raylib.DrawText(confirmStr, (int)_confirmNameBtn.X + (int)((_confirmNameBtn.Width - Raylib.MeasureText(confirmStr, 18)) / 2), (int)_confirmNameBtn.Y + 16, 18, Color.Black);
            }
            else if (_currentState == GameState.MainMenu)
            {
                Raylib.DrawRectangleGradientV(0, 0, VirtualWidth, VirtualHeight, new Color(15, 23, 42, 255), new Color(30, 41, 59, 255));
                foreach (var p in _backgroundParticles)
                {
                    Raylib.DrawCircleV(p.Position, p.Size, p.Color);
                }

                Raylib.DrawRectangle(80, 50, 640, 600, new Color(15, 23, 42, 200));
                Raylib.DrawRectangleLines(80, 50, 640, 600, new Color(51, 65, 85, 255));

                string titleText = "MEMORY ROASTER 3000";
                int titleWidth = Raylib.MeasureText(titleText, 34);
                Raylib.DrawText(titleText, (VirtualWidth - titleWidth) / 2, 75, 34, Color.SkyBlue);

                string playerWelcome = $"Player: {GetActivePlayerName()}";
                int welcomeWidth = Raylib.MeasureText(playerWelcome, 20);
                Raylib.DrawText(playerWelcome, (VirtualWidth - welcomeWidth) / 2, 120, 20, Color.Gold);

                string subText = "Select difficulty mode:";
                int subWidth = Raylib.MeasureText(subText, 18);
                Raylib.DrawText(subText, (VirtualWidth - subWidth) / 2, 150, 18, Color.LightGray);

                bool hoverEasy = IsHovered(inputPos, _btnEasy);
                bool hoverNormal = IsHovered(inputPos, _btnNormal);
                bool hoverHard = IsHovered(inputPos, _btnHard);

                Color easyColor = hoverEasy ? new Color(22, 163, 74, 255) : new Color(21, 128, 61, 255);
                Color normalColor = hoverNormal ? new Color(234, 179, 8, 255) : new Color(202, 138, 4, 255);
                Color hardColor = hoverHard ? new Color(225, 29, 72, 255) : new Color(190, 18, 60, 255);

                Raylib.DrawRectangle((int)_btnEasy.X, (int)_btnEasy.Y, (int)_btnEasy.Width, (int)_btnEasy.Height, easyColor);
                Raylib.DrawRectangleLines((int)_btnEasy.X, (int)_btnEasy.Y, (int)_btnEasy.Width, (int)_btnEasy.Height, Color.White);
                string easyStr = "EASY (For people with 3 brain cells)";
                Raylib.DrawText(easyStr, (int)_btnEasy.X + (int)((_btnEasy.Width - Raylib.MeasureText(easyStr, 18)) / 2), (int)_btnEasy.Y + 23, 18, Color.White);

                Raylib.DrawRectangle((int)_btnNormal.X, (int)_btnNormal.Y, (int)_btnNormal.Width, (int)_btnNormal.Height, normalColor);
                Raylib.DrawRectangleLines((int)_btnNormal.X, (int)_btnNormal.Y, (int)_btnNormal.Width, (int)_btnNormal.Height, Color.White);
                string normalStr = "NORMAL (You'll probably still fail)";
                Raylib.DrawText(normalStr, (int)_btnNormal.X + (int)((_btnNormal.Width - Raylib.MeasureText(normalStr, 18)) / 2), (int)_btnNormal.Y + 23, 18, Color.White);

                Raylib.DrawRectangle((int)_btnHard.X, (int)_btnHard.Y, (int)_btnHard.Width, (int)_btnHard.Height, hardColor);
                Raylib.DrawRectangleLines((int)_btnHard.X, (int)_btnHard.Y, (int)_btnHard.Width, (int)_btnHard.Height, Color.White);
                string hardStr = "HARD (Prepare to cry)";
                Raylib.DrawText(hardStr, (int)_btnHard.X + (int)((_btnHard.Width - Raylib.MeasureText(hardStr, 18)) / 2), (int)_btnHard.Y + 23, 18, Color.White);

                string footerStr = "Tip: Your session name stays active across all modes until you close the application.";
                Raylib.DrawText(footerStr, (VirtualWidth - Raylib.MeasureText(footerStr, 14)) / 2, 550, 14, Color.Gray);
            }
            else if (_currentState == GameState.Tutorial)
            {
                string name = GetActivePlayerName().ToUpper();
                Raylib.DrawText($"WELCOME, {name}!", 130, 30, 26, Color.DarkBlue);
                Raylib.DrawText("HOW TO NOT EMBARRASS YOURSELF", 130, 70, 24, Color.Black);
                Raylib.DrawText("1. Look at the blue line. Try to actually use your brain.", 100, 140, 20, Color.Black);
                Raylib.DrawText("2. Click start to hide it. Try not to panic immediately.", 100, 185, 20, Color.Black);
                Raylib.DrawText("3. Left-click/touch & drag to redraw what you just forgot.", 100, 230, 20, Color.Black);
                Raylib.DrawText("4. Right-click/reset to erase your miserable mistakes.", 100, 275, 20, Color.DarkBlue);
                Raylib.DrawText("5. If you fail, we will judge you silently. (And loudly).", 100, 320, 20, Color.Maroon);

                Raylib.DrawRectangle((int)_startTutorialBtn.X, (int)_startTutorialBtn.Y, (int)_startTutorialBtn.Width, (int)_startTutorialBtn.Height, Color.DarkGreen);
                Raylib.DrawText("I DARE TO TRY", (int)_startTutorialBtn.X + 110, (int)_startTutorialBtn.Y + 15, 24, Color.White);
            }
            else if (_currentState == GameState.Memorizing)
            {
                string name = GetActivePlayerName().ToUpper();
                Raylib.DrawText($"MEMORIZE THIS, {name}!", 140, 10, 22, Color.DarkBlue);

                for (int x = 0; x < GridSize; x++)
                {
                    for (int y = 0; y < GridSize; y++)
                    {
                        var node = _grid[x, y];
                        Color color = node.IsWalkable ? Color.LightGray : Color.DarkGray;

                        if (_targetSolutionPath != null && _targetSolutionPath.Contains(node))
                        {
                            color = Color.SkyBlue;
                        }

                        Raylib.DrawRectangle(OffsetX + x * CellSize, OffsetY + y * CellSize, CellSize - 2, CellSize - 2, color);
                    }
                }

                Raylib.DrawRectangle(OffsetX + 2 * CellSize, OffsetY + 10 * CellSize, CellSize - 2, CellSize - 2, Color.Green);
                Raylib.DrawRectangle(OffsetX + 18 * CellSize, OffsetY + 10 * CellSize, CellSize - 2, CellSize - 2, Color.Red);

                Raylib.DrawRectangle((int)_startPlayingButton.X, (int)_startPlayingButton.Y, (int)_startPlayingButton.Width, (int)_startPlayingButton.Height, Color.DarkGreen);
                Raylib.DrawText("I'M READY (PROVE ME WRONG)", (int)_startPlayingButton.X + 45, (int)_startPlayingButton.Y + 12, 18, Color.White);
            }
            else if (_currentState == GameState.Playing)
            {
                Raylib.DrawText($"Player: {GetActivePlayerName()} | {_currentMode} Lvl {_currentLevel}/{_maxLevels}", 160, 10, 18, Color.DarkGray);

                float timerPct = _timeRemaining / _maxTimeForLevel;
                Color timerColor = timerPct > 0.4f ? Color.DarkGreen : Color.Maroon;
                Raylib.DrawRectangle(150, 32, (int)(500 * timerPct), 6, timerColor);
                Raylib.DrawText($"Tick-Tock: {_timeRemaining:F1}s", 630, 8, 16, timerColor);

                for (int x = 0; x < GridSize; x++)
                {
                    for (int y = 0; y < GridSize; y++)
                    {
                        var node = _grid[x, y];
                        Color color = node.IsWalkable ? Color.LightGray : Color.DarkGray;

                        if (_userDrawnPath.Contains((x, y)))
                        {
                            color = Color.SkyBlue;
                        }

                        Raylib.DrawRectangle(OffsetX + x * CellSize, OffsetY + y * CellSize, CellSize - 2, CellSize - 2, color);
                    }
                }

                Raylib.DrawRectangle(OffsetX + 2 * CellSize, OffsetY + 10 * CellSize, CellSize - 2, CellSize - 2, Color.Green);
                Raylib.DrawRectangle(OffsetX + 18 * CellSize, OffsetY + 10 * CellSize, CellSize - 2, CellSize - 2, Color.Red);

                if (!string.IsNullOrEmpty(_statusMessage))
                {
                    Raylib.DrawText(_statusMessage, 160, 580, 18, Color.Maroon);
                }

                Raylib.DrawRectangle((int)_checkButton.X, (int)_checkButton.Y, (int)_checkButton.Width, (int)_checkButton.Height, Color.DarkGreen);
                Raylib.DrawText("CHECK WORK", (int)_checkButton.X + 22, (int)_checkButton.Y + 12, 16, Color.White);

                Raylib.DrawRectangle((int)_resetButton.X, (int)_resetButton.Y, (int)_resetButton.Width, (int)_resetButton.Height, Color.DarkBlue);
                Raylib.DrawText("Wipe Shame", (int)_resetButton.X + 30, (int)_resetButton.Y + 12, 16, Color.White);

                Raylib.DrawRectangle((int)_menuButton.X, (int)_menuButton.Y, (int)_menuButton.Width, (int)_menuButton.Height, Color.Maroon);
                Raylib.DrawText("Give Up", (int)_menuButton.X + 48, (int)_menuButton.Y + 12, 16, Color.White);
            }
            else if (_currentState == GameState.Victory)
            {
                Raylib.DrawRectangle(80, 110, 640, 420, Color.RayWhite);
                Raylib.DrawRectangleLines(80, 110, 640, 420, Color.DarkGreen);

                Raylib.DrawText(_currentVictoryHeadline, 100, 140, 22, Color.DarkGreen);
                Raylib.DrawText($"Mode: {_currentMode} | Lvl {_currentLevel} | Stage {_currentGame}", 100, 185, 18, Color.Black);
                Raylib.DrawText(_currentVictorySubtext, 100, 230, 16, Color.DarkGray);

                Raylib.DrawRectangle((int)_nextGameButton.X, (int)_nextGameButton.Y, (int)_nextGameButton.Width, (int)_nextGameButton.Height, Color.DarkBlue);
                Raylib.DrawText(_currentVictoryBtnText, (int)_nextGameButton.X + (int)((_nextGameButton.Width - Raylib.MeasureText(_currentVictoryBtnText, 20)) / 2), (int)_nextGameButton.Y + 15, 20, Color.White);

                Raylib.DrawRectangle((int)_victoryMenuButton.X, (int)_victoryMenuButton.Y, (int)_victoryMenuButton.Width, (int)_victoryMenuButton.Height, Color.Maroon);
                string fleeText = "Flee Back to Menu";
                Raylib.DrawText(fleeText, (int)_victoryMenuButton.X + (int)((_victoryMenuButton.Width - Raylib.MeasureText(fleeText, 20)) / 2), (int)_victoryMenuButton.Y + 15, 20, Color.White);
            }
            else if (_currentState == GameState.GameOver)
            {
                Raylib.DrawRectangle(80, 110, 640, 420, Color.RayWhite);
                Raylib.DrawRectangleLines(80, 110, 640, 420, Color.Maroon);

                Raylib.DrawText(_currentGameOverHeadline, 100, 140, 22, Color.Maroon);
                Raylib.DrawText($"You ran out of time while staring blankly, {GetActivePlayerName()}.", 100, 190, 18, Color.Black);
                Raylib.DrawText(_currentGameOverSubtext, 100, 235, 16, Color.DarkGray);

                Raylib.DrawRectangle((int)_nextGameButton.X, (int)_nextGameButton.Y, (int)_nextGameButton.Width, (int)_nextGameButton.Height, Color.DarkBlue);
                string retryText = "Embarrass Yourself Again";
                Raylib.DrawText(retryText, (int)_nextGameButton.X + (int)((_nextGameButton.Width - Raylib.MeasureText(retryText, 20)) / 2), (int)_nextGameButton.Y + 15, 20, Color.White);

                Raylib.DrawRectangle((int)_victoryMenuButton.X, (int)_victoryMenuButton.Y, (int)_victoryMenuButton.Width, (int)_victoryMenuButton.Height, Color.Maroon);
                string abandonText = "Run Away to Main Menu";
                Raylib.DrawText(abandonText, (int)_victoryMenuButton.X + (int)((_victoryMenuButton.Width - Raylib.MeasureText(abandonText, 20)) / 2), (int)_victoryMenuButton.Y + 15, 20, Color.White);
            }

            Raylib.EndTextureMode();

            // 2. Scale & Render the Virtual Texture Canvas to Screen Display (Letterboxed)
            float scale = Math.Min(
                (float)Raylib.GetScreenWidth() / VirtualWidth,
                (float)Raylib.GetScreenHeight() / VirtualHeight
            );

            Rectangle destRect = new Rectangle(
                (Raylib.GetScreenWidth() - (VirtualWidth * scale)) * 0.5f,
                (Raylib.GetScreenHeight() - (VirtualHeight * scale)) * 0.5f,
                VirtualWidth * scale,
                VirtualHeight * scale
            );

            Rectangle sourceRect = new Rectangle(0, 0, VirtualWidth, -VirtualHeight);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            Raylib.DrawTexturePro(_targetCanvas.Texture, sourceRect, destRect, Vector2.Zero, 0.0f, Color.White);
            Raylib.EndDrawing();
        }

        public void Stop()
        {
            Raylib.UnloadRenderTexture(_targetCanvas);
            Raylib.CloseWindow();
        }

        public void Run()
        {
            Initialize();

            EngineTime timer = new EngineTime();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (!Raylib.WindowShouldClose())
            {
                double elapsed = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();
                timer.UpdateTime(elapsed);

                Update(timer.DeltaTime);
                Render();
            }

            Stop();
        }
    }
}