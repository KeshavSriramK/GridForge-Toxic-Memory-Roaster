using Android.App;
using Android.Content;
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

    public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
    {
        if (keyCode == Keycode.Back)
        {
            ShowSarcasticExitDialog();
            return true;
        }
        return base.OnKeyDown(keyCode, e);
    }

    private void ShowSarcasticExitDialog()
    {
        string[] titles = {
            "RAGE QUITTING ALREADY?",
            "BRAIN OVERLOAD?",
            "GIVING UP SO SOON?",
            "EGO TOO FRAGILE?"
        };

        string[] messages = {
            "Is your short-term memory failing you, or are you just scared of a 20x20 grid?",
            "Your brain cells called. They need a break, but are you really going to let a puzzle beat you?",
            "Running back to easier apps? We won't judge... much.",
            "Are you sure you want to exit? Your path accuracy wasn't looking too hot anyway."
        };

        var random = new System.Random();
        int index = random.Next(titles.Length);

        RunOnUiThread(() =>
        {
            new AlertDialog.Builder(this)
                .SetTitle(titles[index])
                .SetMessage(messages[index])
                .SetPositiveButton("Flee Like A Coward", (sender, e) =>
                {
                    Finish();
                })
                .SetNegativeButton("Stay & Suffer More", (sender, e) => { })
                .SetCancelable(true)
                .Show();
        });
    }
}