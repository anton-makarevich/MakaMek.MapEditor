using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Sanet.MakaMek.MapEditor.Android.DependencyInjection;
using Sanet.MVVM.DI.Avalonia.Extensions;

namespace Sanet.MakaMek.MapEditor.Android;

[Application]
public class AndroidApp : AvaloniaAndroidApplication<App>
{
    protected AndroidApp(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .UseDependencyInjection(services => services.RegisterPlatformServices())
            .WithInterFont();
    }
}
