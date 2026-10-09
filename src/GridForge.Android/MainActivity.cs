using System;
using System.Collections.Generic;
using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;

namespace GridForge.Android;

[Activity(
    Label = "GridForge",
    Icon = "@mipmap/icon",
    RoundIcon = "@mipmap/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public class MainActivity : Activity
{
    private GameView? _gameView;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        _gameView = new GameView(this);
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

public class GameView : SurfaceView, ISurfaceHolderCallback
{
    private GameThread? _thread;
    private readonly List<MemoryNode> _nodes = new();
    private readonly Random _random = new();
    private int _score = 0;
    private int _lives = 3;
    private float _spawnTimer = 0;

    public GameView(Activity context) : base(context)
    {
        Holder?.AddCallback(this);
        Focusable = true;
    }

    public void SurfaceCreated(ISurfaceHolder holder)
    {
        _thread = new GameThread(Holder, this);
        _thread.Running = true;
        _thread.Start();
    }

    public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height) { }

    public void SurfaceDestroyed(ISurfaceHolder holder)
    {
        Stop();
    }

    public void Start()
    {
        if (_thread == null)
        {
            _thread = new GameThread(Holder, this);
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
                try
                {
                    _thread.Join();
                    break;
                }
                catch { }
            }
            _thread = null;
        }
    }

    public override bool OnTouchEvent(MotionEvent? e)
    {
        if (e?.Action == MotionEventActions.Down)
        {
            float touchX = e.GetX();
            float touchY = e.GetY();

            if (_lives <= 0)
            {
                // Restart game on tap when Game Over
                _score = 0;
                _lives = 3;
                lock (_nodes) { _nodes.Clear(); }
                return true;
            }

            lock (_nodes)
            {
                for (int i = _nodes.Count - 1; i >= 0; i--)
                {
                    var node = _nodes[i];
                    float dx = touchX - node.X;
                    float dy = touchY - node.Y;
                    if (Math.Sqrt(dx * dx + dy * dy) <= node.Radius + 30)
                    {
                        _nodes.RemoveAt(i);
                        _score += 100;
                        return true;
                    }
                }
            }
        }
        return true;
    }

    public void Update(float deltaTime)
    {
        if (_lives <= 0) return;

        _spawnTimer += deltaTime;
        if (_spawnTimer >= 0.8f) // Spawn node every 0.8s
        {
            _spawnTimer = 0;
            lock (_nodes)
            {
                _nodes.Add(new MemoryNode(
                    _random.Next(100, Math.Max(200, Width - 100)),
                    _random.Next(200, Math.Max(300, Height - 200)),
                    60,
                    1.5f + (float)_random.NextDouble() * 1.5f
                ));
            }
        }

        lock (_nodes)
        {
            for (int i = _nodes.Count - 1; i >= 0; i--)
            {
                var node = _nodes[i];
                node.Radius += node.GrowRate;
                if (node.Radius > 180) // Max size before exploding
                {
                    _nodes.RemoveAt(i);
                    _lives--;
                }
            }
        }
    }

    public new void OnDraw(Canvas canvas)
    {
        base.OnDraw(canvas);

        // Dark Background
        canvas.DrawColor(Color.ParseColor("#0D1117"));

        using var paint = new Paint();
        paint.AntiAlias = true;

        if (_lives <= 0)
        {
            paint.Color = Color.ParseColor("#FF7B72");
            paint.TextSize = 70;
            paint.TextAlign = Paint.Align.Center;
            canvas.DrawText("SYSTEM OVERLOAD!", Width / 2f, Height / 2f - 40, paint);

            paint.Color = Color.White;
            paint.TextSize = 40;
            canvas.DrawText($"Final Score: {_score}", Width / 2f, Height / 2f + 40, paint);
            canvas.DrawText("Tap anywhere to reboot", Width / 2f, Height / 2f + 120, paint);
            return;
        }

        // Draw Nodes (Code Memory Leaks)
        lock (_nodes)
        {
            foreach (var node in _nodes)
            {
                paint.Color = Color.ParseColor("#58A6FF");
                paint.SetStyle(Paint.Style.Stroke);
                paint.StrokeWidth = 8;
                canvas.DrawCircle(node.X, node.Y, node.Radius, paint);

                paint.Color = Color.ParseColor("#1F6FEB");
                paint.SetStyle(Paint.Style.Fill);
                canvas.DrawCircle(node.X, node.Y, node.Radius - 10, paint);
            }
        }

        // Draw HUD
        paint.Color = Color.ParseColor("#3FB950");
        paint.TextSize = 48;
        paint.SetStyle(Paint.Style.Fill);
        paint.TextAlign = Paint.Align.Left;
        canvas.DrawText($"SCORE: {_score}", 40, 80, paint);

        paint.Color = Color.ParseColor("#F85149");
        canvas.DrawText($"LIVES: {_lives}", Width - 240, 80, paint);
    }
}

public class MemoryNode
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Radius { get; set; }
    public float GrowRate { get; set; }

    public MemoryNode(float x, float y, float radius, float growRate)
    {
        X = x;
        Y = y;
        Radius = radius;
        GrowRate = growRate;
    }
}

public class GameThread : Thread
{
    private readonly ISurfaceHolder _holder;
    private readonly GameView _view;
    public bool Running { get; set; }

    public GameThread(ISurfaceHolder holder, GameView view)
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
                if (canvas != null)
                {
                    _holder.UnlockCanvasAndPost(canvas);
                }
            }
        }
    }
}