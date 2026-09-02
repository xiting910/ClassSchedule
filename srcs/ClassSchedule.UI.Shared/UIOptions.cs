using ClassSchedule.Infrastructure;
using ClassSchedule.UI.Shared.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json.Nodes;

namespace ClassSchedule.UI.Shared;

/// <summary>
/// UI 选项类
/// </summary>
public sealed class UIOptions
{
    /// <summary>
    /// 主题模式
    /// </summary>
    public ThemeMode Theme
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveToFile();
            }
        }
    }

    /// <summary>
    /// Toast 同时显示的最大条数
    /// </summary>
    public int MaxToastCount
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveToFile();
            }
        }
    }

    /// <summary>
    /// Toast 提示显示时间
    /// </summary>
    public double ToastDurationSeconds
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SaveToFile();
            }
        }
    }

    /// <summary>
    /// 构造函数, 从应用程序配置对象中获取 UI 配置
    /// </summary>
    /// <param name="configuration">应用程序配置对象</param>
    public UIOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(UIOptions));

        const ThemeMode DefaultTheme = ThemeMode.System;
        Theme = Enum.TryParse(section[nameof(Theme)], out ThemeMode theme) ? theme : DefaultTheme;

        const int DefaultMaxToastCount = 2, MaxMaxToastCount = 5;
        MaxToastCount = int.TryParse(section[nameof(MaxToastCount)], out var maxToastCount)
            ? Math.Clamp(maxToastCount, 0, MaxMaxToastCount)
            : DefaultMaxToastCount;

        const double DefaultToastDurationSeconds = 5.0, MaxToastDurationSeconds = 10.0;
        ToastDurationSeconds = double.TryParse(section[nameof(ToastDurationSeconds)], out var seconds)
            ? Math.Clamp(seconds, 0.0, MaxToastDurationSeconds)
            : DefaultToastDurationSeconds;
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
                [nameof(Theme)] = Theme.ToString(),
                [nameof(MaxToastCount)] = MaxToastCount,
                [nameof(ToastDurationSeconds)] = ToastDurationSeconds
            }
        }.ToJsonString(FileSystem.JsonSerializerOptions);
        FileSystem.SafeWriteToFile(FileSystem.Settings, UISettingsFileName, content);
    }
}
