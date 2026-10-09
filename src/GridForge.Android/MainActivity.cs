using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace GridForge.Android;

[Activity(
    Label = "GridForge",
    MainLauncher = true,
    Icon = "@mipmap/icon",
    RoundIcon = "@mipmap/icon",
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ScreenOrientation = ScreenOrientation.SensorLandscape,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
public class MainActivity : AndroidGameActivity
{
    private MonoGameApp? _game;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        _game = new MonoGameApp();
        var mainView = (View)_game.Services.GetService(typeof(View))!;
        SetContentView(mainView);
        _game.Run();
    }
}