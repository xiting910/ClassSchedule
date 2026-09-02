using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json.Nodes;

namespace ClassSchedule.Infrastructure;

/// <summary>
/// 文件日志记录器选项类
/// </summary>
public sealed class FileLoggerOptions
{
    /// <summary>
    /// 最大日志文件数量
    /// </summary>
    public int MaxLogFileCount
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
    /// 最小日志级别
    /// </summary>
    public LogLevel MinLevel
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
    /// 构造函数, 从配置中读取选项
    /// </summary>
    /// <param name="configuration">配置对象</param>
    public FileLoggerOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(FileLoggerOptions));

        const int DefaultMaxLogFileCount = 5, MinMaxLogFileCount = 1, MaxMaxLogFileCount = 10;
        MaxLogFileCount = int.TryParse(section[nameof(MaxLogFileCount)], out var maxLogFileCount)
            ? Math.Clamp(maxLogFileCount, MinMaxLogFileCount, MaxMaxLogFileCount)
            : DefaultMaxLogFileCount;

        const LogLevel DefaultLevel = LogLevel.Information;
        MinLevel = Enum.TryParse(section[nameof(MinLevel)], out LogLevel minLevel) ? minLevel : DefaultLevel;
    }

    /// <summary>
    /// 将当前选项保存到文件中
    /// </summary>
    private void SaveToFile()
    {
        const string LogSettingsFileName = $"LogSettings{FileSystem.JsonFileSuffix}";
        var content = new JsonObject
        {
            [nameof(FileLoggerOptions)] = new JsonObject
            {
                [nameof(MaxLogFileCount)] = MaxLogFileCount,
                [nameof(MinLevel)] = MinLevel.ToString()
            }
        }.ToJsonString(FileSystem.JsonSerializerOptions);
        FileSystem.SafeWriteToFile(FileSystem.Settings, LogSettingsFileName, content);
    }
}
