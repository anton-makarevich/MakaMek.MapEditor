using Android.Content.PM;
using Android.Views;
using Avalonia.Android;

namespace Sanet.MakaMek.MapEditor.Android;

[Activity(
    Label = "MakaMap",
    Theme = "@style/MyTheme.SplashScreen",
    Icon = "@mipmap/ic_launcher",
    RoundIcon = "@mipmap/ic_launcher_round",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        AndroidX.Core.SplashScreen.SplashScreen.InstallSplashScreen(this);
        base.OnCreate(savedInstanceState);

        // Make the status bar transparent and ensure content can go behind it
        // Set the status bar to be hidden
        Window?.AddFlags(WindowManagerFlags.Fullscreen);
    }
}
