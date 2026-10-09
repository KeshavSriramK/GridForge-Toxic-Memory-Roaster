using System;
using global::Android.App;
using global::Android.Content;
using global::Android.Content.PM;
using global::Android.OS;
using global::Android.Text;
using global::Android.Views;
using global::Android.Views.InputMethods;
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
    // AdjustNothing: the keyboard must NOT resize/shift the game view.
    WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.StateHidden,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
public class MainActivity : AndroidGameActivity
{
    private MonoGameApp? _game;
    private View? _gameView;

    private EditText? _nameField;
    private bool _nameFieldClosed;
    private static readonly Random Rng = new();

    /// <summary>Latest text typed in the name box (safe to read from the game thread).</summary>
    public string NameFieldText { get; private set; } = "";

    public int GameViewWidth => _gameView?.Width ?? 0;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        _game = new MonoGameApp(this);
        var mainView = (View)_game.Services.GetService(typeof(View))!;
        _gameView = mainView;
        SetContentView(mainView);
        _game.Run();
    }

    // ------------------------------------------------------------------
    // Safe area (status bar, taskbar, navigation bar, camera cut-out)
    // ------------------------------------------------------------------
    /// <summary>Returns { left, top, right, bottom } insets in view pixels.</summary>
    public int[] GetSafeInsets()
    {
        try
        {
            var insets = Window?.DecorView?.RootWindowInsets;
            if (insets == null) return new[] { 0, 0, 0, 0 };

            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                var i = insets.GetInsetsIgnoringVisibility(
                    WindowInsets.Type.SystemBars() | WindowInsets.Type.DisplayCutout());
                return new[] { i.Left, i.Top, i.Right, i.Bottom };
            }

#pragma warning disable CA1422
            return new[]
            {
                insets.SystemWindowInsetLeft, insets.SystemWindowInsetTop,
                insets.SystemWindowInsetRight, insets.SystemWindowInsetBottom
            };
#pragma warning restore CA1422
        }
        catch
        {
            return new[] { 0, 0, 0, 0 };
        }
    }

    // ------------------------------------------------------------------
    // Inline name field (a real EditText laid over the yellow box)
    // x/y/w/h are BACKBUFFER pixels; they are converted to view pixels here.
    // ------------------------------------------------------------------
    public void ShowNameField(int x, int y, int w, int h, int backbufferWidth, float fontPx)
    {
        RunOnUiThread(() =>
        {
            if (_nameFieldClosed) return;

            float f = (_gameView != null && _gameView.Width > 0 && backbufferWidth > 0)
                ? _gameView.Width / (float)backbufferWidth
                : 1f;

            var lp = new FrameLayout.LayoutParams(Math.Max(1, (int)(w * f)), Math.Max(1, (int)(h * f)))
            {
                LeftMargin = (int)(x * f),
                TopMargin = (int)(y * f),
                Gravity = GravityFlags.Top | GravityFlags.Left
            };

            if (_nameField == null)
            {
                var field = new EditText(this)
                {
                    Hint = "TAP HERE TO TYPE YOUR NAME",
                    Gravity = GravityFlags.Center
                };
                field.SetSingleLine(true);
                field.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
                field.SetPadding(4, 0, 4, 0);
                field.SetTextColor(global::Android.Graphics.Color.Rgb(255, 215, 0));
                field.SetHintTextColor(global::Android.Graphics.Color.Argb(170, 255, 215, 0));
                field.SetTypeface(global::Android.Graphics.Typeface.Monospace, global::Android.Graphics.TypefaceStyle.Bold);
                field.SetTextSize(global::Android.Util.ComplexUnitType.Px, fontPx * f);
                field.InputType = InputTypes.ClassText | InputTypes.TextFlagCapCharacters | InputTypes.TextFlagNoSuggestions;
                // Done key closes the keyboard (and no full-screen "extract" editor in landscape).
                field.ImeOptions = (ImeAction)((int)ImeAction.Done | (int)ImeFlags.NoExtractUi);
                field.SetFilters(new IInputFilter[] { new InputFilterLengthFilter(12) });
                field.TextChanged += (s, e) => NameFieldText = field.Text ?? "";
                field.EditorAction += (s, e) =>
                {
                    if (e.ActionId == ImeAction.Done)
                    {
                        HideKeyboardInternal();
                        e.Handled = true;
                    }
                };

                _nameField = field;
                AddContentView(field, lp);
            }
            else
            {
                _nameField.LayoutParameters = lp;
                _nameField.SetTextSize(global::Android.Util.ComplexUnitType.Px, fontPx * f);
            }
        });
    }

    public void HideKeyboard() => RunOnUiThread(HideKeyboardInternal);

    private void HideKeyboardInternal()
    {
        if (_nameField == null) return;
        var imm = (InputMethodManager?)GetSystemService(InputMethodService);
        imm?.HideSoftInputFromWindow(_nameField.WindowToken, HideSoftInputFlags.None);
        _nameField.ClearFocus();
    }

    public void RemoveNameField()
    {
        RunOnUiThread(() =>
        {
            _nameFieldClosed = true;
            HideKeyboardInternal();
            if (_nameField != null)
            {
                (_nameField.Parent as ViewGroup)?.RemoveView(_nameField);
                _nameField = null;
            }
            NameFieldText = "";
        });
    }

    // ------------------------------------------------------------------
    // Sarcastic name generator + "are you sure?" dialog
    // ------------------------------------------------------------------
    private static readonly string[] NameSuffixes = {
        "THE FORGETFUL", "GOLDFISH BRAIN", "404 BRAIN", "THE UNREMARKABLE", "2 BRAIN CELLS",
        "THE LOADING...", "NPC ENERGY", "WHO ASKED", "THE CLUELESS", "THE HOPELESS",
        "BUFFERING...", "MEMORY LEAK", "THE DISAPPOINTMENT", "PARTICIPANT ONLY",
        "THE BLANK STARE", "TOASTER BRAIN"
    };

    private static readonly string[] RandomFullNames = {
        "CAPTAIN CLUELESS", "SIR FORGETSALOT", "ERROR 404: NO BRAIN", "CHIEF FORGETFUL",
        "PROFESSOR BLANKSLATE", "LORD OF NOTHING", "DR. WHO DIS?", "GENERAL DISASTER",
        "MAYOR OF FAILTOWN", "GRANDMASTER OOPS", "BARON VON BLANK", "SIR LAGS A LOT"
    };

    private static string BuildSarcasticName(string typed)
    {
        if (string.IsNullOrWhiteSpace(typed))
            return RandomFullNames[Rng.Next(RandomFullNames.Length)];
        return typed.Trim() + " " + NameSuffixes[Rng.Next(NameSuffixes.Length)];
    }

    public void ShowNameVerdict(string typed, Action<string> onAccepted)
    {
        RunOnUiThread(() =>
        {
            bool empty = string.IsNullOrWhiteSpace(typed);
            string newName = BuildSarcasticName(typed);
            string title;
            string message;

            if (empty)
            {
                title = "TOO LAZY TO TYPE?";
                string[] msgs = {
                    $"You typed NOTHING. Impressive commitment to the bare minimum.\n\nWe picked for you:\n{newName}\n\nFrom now on that is your name. Keep your amazing new name, or reroll and pretend you have a choice?",
                    $"Even a name was too much effort? Our computer scanned your brain and found:\n{newName}\n\nEveryone will see it. Keep it, or tempt fate again?",
                    $"No name? Bold. Your new identity is:\n{newName}\n\nAre you SURE you want to change your name, or will you keep this masterpiece?"
                };
                message = msgs[Rng.Next(msgs.Length)];
            }
            else
            {
                title = "NAME UPGRADED!";
                string[] msgs = {
                    $"\"{typed.Trim()}\" was far too boring, so we fixed it.\n\nYou are now:\n{newName}\n\nCongratulations. You're welcome.",
                    $"We improved your name:\n{newName}\n\nHonestly it's still the best part of you. Keep it?",
                    $"Nobody is scared of \"{typed.Trim()}\". So meet the new you:\n{newName}\n\nKeep your amazing new name, or reroll?"
                };
                message = msgs[Rng.Next(msgs.Length)];
            }

            new AlertDialog.Builder(this)
                .SetTitle(title)
                .SetMessage(message)
                .SetCancelable(false)
                .SetPositiveButton("KEEP MY AMAZING NEW NAME", (s, a) => onAccepted(newName))
                .SetNegativeButton("REROLL (I'M NOT READY)", (s, a) => ShowNameVerdict(typed, onAccepted))
                .SetNeutralButton("LET ME TYPE (FINE)", (s, a) => { })
                .Show();
        });
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
            int idx = Rng.Next(titles.Length);

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

        int index = Rng.Next(titles.Length);

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