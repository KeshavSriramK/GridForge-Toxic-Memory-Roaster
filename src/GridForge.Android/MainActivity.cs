using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Android.Graphics;
using Android.Views;

namespace GridForge.Android;

[Activity(
    Label = "GridForge",
    Icon = "@mipmap/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var layout = new LinearLayout(this)
        {
            Orientation = Orientation.Vertical
        };
        layout.SetGravity(GravityFlags.Center);
        layout.SetBackgroundColor(Color.ParseColor("#0D1117"));

        var coverView = new ImageView(this);
        coverView.SetImageResource(Resource.Drawable.cover);
        coverView.SetAdjustViewBounds(true);
        coverView.SetScaleType(ImageView.ScaleType.FitCenter);

        var titleText = new TextView(this)
        {
            Text = "⚡ GridForge Engine Loaded!",
            TextSize = 20,
            Gravity = GravityFlags.Center
        };
        titleText.SetTextColor(Color.ParseColor("#58A6FF"));
        titleText.SetPadding(0, 32, 0, 0);

        layout.AddView(coverView);
        layout.AddView(titleText);

        SetContentView(layout);
    }
}