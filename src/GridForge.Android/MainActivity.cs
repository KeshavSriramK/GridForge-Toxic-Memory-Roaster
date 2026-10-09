using System;
using global::Android.App;
using global::Android.Content;
using global::Android.Content.PM;
using global::Android.OS;
using global::Android.Views;
using global::Android.Widget;
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

        _game = new MonoGameApp(this);
        var mainView = (View)_game.Services.GetService(typeof(View))!;
        SetContentView(mainView);
        _game.Run();
    }

    public void PromptForPlayerName(Action<string> onNameEntered)
    {
        RunOnUiThread(() =>
        {
            var layout = new LinearLayout(this)
            {
                Orientation = global::Android.Widget.Orientation.Vertical
            };
            layout.SetPadding(60, 50, 60, 50);

            var input = new EditText(this)
            {
                Hint = "Type your name here...",
                TextSize = 22f
            };
            layout.AddView(input);

            new AlertDialog.Builder(this)
                .SetTitle("WHO IS DARING TO SUFFER?")
                .SetMessage("Type your name so we know whose ego is about to be crushed. (Leave blank for a random sarcastic title):")
                .SetView(layout)
                .SetPositiveButton("Register Failure", (sender, args) =>
                {
                    string name = input.Text?.Trim() ?? "";
                    if (string.IsNullOrEmpty(name))
                    {
                        string[] fallbackRoasts = {
                            "CAPTAIN CLUELESS",
                            "SIR FORGETSALOT",
                            "ERROR 404: BRAIN NOT FOUND",
                            "CHIEF FORGETFUL",
                            "MASTER OF FORGETFULNESS",
                            "PROFESSOR BLANKSLATE"
                        };
                        name = fallbackRoasts[new System.Random().Next(fallbackRoasts.Length)];

                        new AlertDialog.Builder(this)
                            .SetTitle("TOO LAZY TO TYPE?")
                            .SetMessage(CrawlRandomRoastMessage(name))
                            .SetPositiveButton("Accept My Fate", (s, a) => onNameEntered(name))
                            .Show();
                    }
                    else
                    {
                        onNameEntered(name);
                    }
                })
                .SetCancelable(false)
                .Show();
        });
    }

    private string CrawlRandomRoastMessage(string assignedName)
    {
        string[] messages = {
            $"Fine! Since you refused to name yourself, your randomly assigned title is now {assignedName}. Enjoy your shame.",
            $"Too lazy to type? Meet {assignedName}. Wear it like a badge of honor.",
            $"Skipping the keyboard step? Bold strategy. Welcome to the game, {assignedName}."
        };
        return messages[new System.Random().Next(messages.Length)];
    }

    public void ShowSurrenderDialog(Action onConfirmed)
    {
        RunOnUiThread(() =>
        {
            string[] titles = {
                "QUITTING ALREADY?",
                "COWARDICE DETECTED!",
                "EGO SURRENDER?",
                "GIVING UP SO SOON?",
                "ABANDONING SHIP?"
            };
            string[] messages = {
                "Are you sure you want to run away to the main menu like a frightened toddler?",
                "Giving up already? Your ancestors are shaking their heads.",
                "Ah, fleeing the scene of the crime before anyone sees your score.",
                "Running back to safety? The 20x20 grid was too much for your fragile brain.",
                "Crying uncle already? We expected nothing less."
            };
            int idx = new System.Random().Next(titles.Length);

            new AlertDialog.Builder(this)
                .SetTitle(titles[idx])
                .SetMessage(messages[idx])
                .SetPositiveButton("Flee in Shame", (sender, args) => onConfirmed())
                .SetNegativeButton("Stay & Suffer More", (sender, args) => { })
                .SetCancelable(true)
                .Show();
        });
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
            "EGO TOO FRAGILE?",
            "RUNNING AWAY?"
        };

        string[] messages = {
            "Is your short-term memory failing you, or are you just scared of a 20x20 grid?",
            "Your brain cells called. They need a break, but are you really going to let a puzzle beat you?",
            "Running back to easier apps? We won't judge... much.",
            "Are you sure you want to exit? Your path accuracy wasn't looking too hot anyway.",
            "Fleeing the app before your high score crashes into negative numbers? Smart choice."
        };

        int index = new System.Random().Next(titles.Length);

        RunOnUiThread(() =>
        {
            new AlertDialog.Builder(this)
                .SetTitle(titles[index])
                .SetMessage(messages[index])
                .SetPositiveButton("Flee Like A Coward", (sender, e) => Finish())
                .SetNegativeButton("Stay & Suffer More", (sender, e) => { })
                .SetCancelable(true)
                .Show();
        });
    }
}