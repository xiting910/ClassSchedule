using ClassSchedule.Infrastructure;
using ClassSchedule.UI.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json.Nodes;

namespace ClassSchedule.UI.Shared;

/// <summary>
/// UI 选项类
/// </summary>
public sealed partial class UIOptions
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<UIOptions> _logger;

    /// <summary>
    /// 主题模式字段
    /// </summary>
    private ThemeMode _theme;

    /// <summary>
    /// 主题模式
    /// </summary>
    public ThemeMode Theme
    {
        get => _theme;
        set
        {
            if (_theme != value)
            {
                LogThemeChanged(_theme, value);
                _theme = value;
                SaveToFile();
            }
        }
    }

    /// <summary>
    /// <see cref="Toast"/> 同时显示的最大条数字段
    /// </summary>
    private int _maxToastCount;

    /// <summary>
    /// <see cref="Toast"/> 同时显示的最大条数
    /// </summary>
    public int MaxToastCount
    {
        get => _maxToastCount;
        set
        {
            if (_maxToastCount != value)
            {
                LogMaxToastCountChanged(_maxToastCount, value);
                _maxToastCount = value;
                SaveToFile();
            }
        }
    }

    /// <summary>
    /// <see cref="Toast"/> 显示时长字段
    /// </summary>
    private double _toastDurationSeconds;

    /// <summary>
    /// <see cref="Toast"/> 显示时长
    /// </summary>
    public double ToastDurationSeconds
    {
        get => _toastDurationSeconds;
        set
        {
            if (_toastDurationSeconds != value)
            {
                LogToastDurationSecondsChanged(_toastDurationSeconds, value);
                _toastDurationSeconds = value;
                SaveToFile();
            }
        }
    }

    /// <summary>
    /// 构造函数, 注入日志记录器和配置对象, 并从配置中读取选项
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="configuration">应用程序配置对象</param>
    public UIOptions(ILogger<UIOptions> logger, IConfiguration configuration)
    {
        _logger = logger;
        var section = configuration.GetSection(nameof(UIOptions));

        const ThemeMode DefaultTheme = ThemeMode.System;
        _theme = Enum.TryParse(section[nameof(Theme)], out ThemeMode theme) ? theme : DefaultTheme;

        const int DefaultMaxToastCount = 2, MaxMaxToastCount = 5;
        _maxToastCount = int.TryParse(section[nameof(MaxToastCount)], out var maxToastCount)
            ? Math.Clamp(maxToastCount, 0, MaxMaxToastCount)
            : DefaultMaxToastCount;

        const double DefaultToastDurationSeconds = 5.0, MaxToastDurationSeconds = 10.0;
        _toastDurationSeconds = double.TryParse(section[nameof(ToastDurationSeconds)], out var seconds)
            ? Math.Clamp(seconds, 0.0, MaxToastDurationSeconds)
            : DefaultToastDurationSeconds;

        LogInitialized(_theme, _maxToastCount, _toastDurationSeconds);
    }

    /// <summary>
    /// 将当前配置保存到文件
    /// </summary>
    private void SaveToFile()
    {
        const string UISettingsFileName = $"UISettings{FileSystem.JsonFileSuffix}";
        var content = new JsonObject
        {
            [nameof(UIOptions)] = new JsonObject
            {
                [nameof(Theme)] = _theme.ToString(),
                [nameof(MaxToastCount)] = _maxToastCount,
                [nameof(ToastDurationSeconds)] = _toastDurationSeconds
            }
        }.ToJsonString(FileSystem.JsonSerializerOptions);

        try
        {
            FileSystem.WriteToFile(FileSystem.Settings, UISettingsFileName, content);
            LogSavedToFile();
        }
        catch (Exception ex)
        {
            LogSaveToFileFailed(ex);
        }
    }

    /// <summary>
    /// 记录初始化完成的日志
    /// </summary>
    /// <param name="theme">主题模式</param>
    /// <param name="maxToastCount">Toast 最大显示条数</param>
    /// <param name="toastDurationSeconds">Toast 显示时长</param>
    [LoggerMessage(
        EventId = 1,
        EventName = "Initialized",
        Level = LogLevel.Information,
        Message = "Initialized from configuration: Theme={Theme}, MaxToastCount={MaxToastCount}, ToastDurationSeconds={ToastDurationSeconds}"
    )]
    private partial void LogInitialized(ThemeMode theme, int maxToastCount, double toastDurationSeconds);

    /// <summary>
    /// 记录保存选项到文件的日志
    /// </summary>
    [LoggerMessage(
        EventId = 2,
        EventName = "SavedToFile",
        Level = LogLevel.Debug,
        Message = "Saved to file"
    )]
    private partial void LogSavedToFile();

    /// <summary>
    /// 记录保存选项到文件时失败的日志
    /// </summary>
    /// <param name="exception">异常对象</param>
    [LoggerMessage(
        EventId = 3,
        EventName = "SaveToFileFailed",
        Level = LogLevel.Warning,
        Message = "Failed to save to file"
    )]
    private partial void LogSaveToFileFailed(Exception exception);

    /// <summary>
    /// 记录主题模式更改的日志
    /// </summary>
    /// <param name="previousValue">更改前的值</param>
    /// <param name="newValue">更改后的值</param>
    [LoggerMessage(
        EventId = 4,
        EventName = "ThemeChanged",
        Level = LogLevel.Information,
        Message = "Theme changed from {PreviousValue} to {NewValue}"
    )]
    private partial void LogThemeChanged(ThemeMode previousValue, ThemeMode newValue);

    /// <summary>
    /// 记录 Toast 最大显示条数更改的日志
    /// </summary>
    /// <param name="previousValue">更改前的值</param>
    /// <param name="newValue">更改后的值</param>
    [LoggerMessage(
        EventId = 5,
        EventName = "MaxToastCountChanged",
        Level = LogLevel.Information,
        Message = "MaxToastCount changed from {PreviousValue} to {NewValue}"
    )]
    private partial void LogMaxToastCountChanged(int previousValue, int newValue);

    /// <summary>
    /// 记录 Toast 显示时长更改的日志
    /// </summary>
    /// <param name="previousValue">更改前的值</param>
    /// <param name="newValue">更改后的值</param>
    [LoggerMessage(
        EventId = 6,
        EventName = "ToastDurationSecondsChanged",
        Level = LogLevel.Information,
        Message = "ToastDurationSeconds changed from {PreviousValue} to {NewValue}"
    )]
    private partial void LogToastDurationSecondsChanged(double previousValue, double newValue);
}
