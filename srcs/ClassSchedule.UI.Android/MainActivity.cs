using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace ClassSchedule.UI.Android;

/// <summary>
/// 主活动类
/// </summary>
[Activity(
    Label = nameof(ClassSchedule),
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode
)]
public class MainActivity : AvaloniaMainActivity;
