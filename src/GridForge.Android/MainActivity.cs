using Android.App;
using Android.Content.PM;
using Android.OS;
using GridForge.Core.Engine;

namespace GridForge.Android
{
    [Activity(
        Label = "GridForge",
        MainLauncher = true,
        Icon = "@mipmap/icon",
        Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation
    )]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            GameApp game = new GameApp();
            game.Run();
        }
    }
}