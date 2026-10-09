using Android.App;
using Android.Content.PM;
using Android.OS;

namespace GridForge.Android;

[Activity(
    Label = "GridForge",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        // Engine initialization code
    }
}