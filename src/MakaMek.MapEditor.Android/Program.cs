using Android.Content.PM;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using Sanet.MVVM.DI.Avalonia.Extensions;

namespace Sanet.MakaMek.MapEditor.Android;

[Activity(
    Label = "MakaMek.MapEditor.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .UseDependencyInjection(_ => {})
            .WithInterFont();
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Make the status bar transparent and ensure content can go behind it
        if (Window != null)
        {
            // Set the status bar to be hidden
            Window.AddFlags(WindowManagerFlags.Fullscreen);
        }
    }
}
