using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace OvoUi.Demo.Android;

[Activity(
    Label = "OvoUi Demo",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
