using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace ClassSchedule.UI.Android;

/// <summary>
/// 主活动类
/// </summary>
[Activity(
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode,
    Icon = "@drawable/icon",
    Label = nameof(ClassSchedule),
    MainLauncher = true,
    Theme = "@style/MyTheme.NoActionBar"
)]
public class MainActivity : AvaloniaMainActivity;
