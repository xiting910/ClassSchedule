using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// <see cref="FileLoggerOptions"/> 的单元测试
/// </summary>
public sealed class FileLoggerOptionsTests
{
    /// <summary>
    /// 日志设置文件的完整路径
    /// </summary>
    private static string LogSettingsFilePath => Path.Combine(
        FileSystem.Settings.FullName, $"LogSettings{FileSystem.JsonFileSuffix}"
    );

    /// <summary>
    /// 验证未配置任何选项时使用默认值: 最大日志文件数量 5, 最小日志级别 Information
    /// </summary>
    [Fact]
    public void Ctor_NoConfiguration_UsesDefaults()
    {
        var options = new FileLoggerOptions(TestConfigurationFactory.Create());

        Assert.Equal(5, options.MaxLogFileCount);
        Assert.Equal(LogLevel.Information, options.MinLevel);
    }

    /// <summary>
    /// 验证最大日志文件数量被限制在 1 到 10 之间
    /// </summary>
    /// <param name="configured">配置值</param>
    /// <param name="expected">期望值</param>
    [Theory]
    [InlineData("0", 1)]
    [InlineData("-3", 1)]
    [InlineData("1", 1)]
    [InlineData("7", 7)]
    [InlineData("10", 10)]
    [InlineData("11", 10)]
    [InlineData("100", 10)]
    public void MaxLogFileCount_IsClamped(string configured, int expected)
    {
        var options = new FileLoggerOptions(TestConfigurationFactory.Create(
            ($"{nameof(FileLoggerOptions)}:{nameof(FileLoggerOptions.MaxLogFileCount)}", configured)
        ));

        Assert.Equal(expected, options.MaxLogFileCount);
    }

    /// <summary>
    /// 验证最大日志文件数量配置为非法文本时使用默认值 5
    /// </summary>
    [Fact]
    public void MaxLogFileCount_InvalidText_FallsBackToDefault()
    {
        var options = new FileLoggerOptions(TestConfigurationFactory.Create(
            ($"{nameof(FileLoggerOptions)}:{nameof(FileLoggerOptions.MaxLogFileCount)}", "not-a-number")
        ));

        Assert.Equal(5, options.MaxLogFileCount);
    }

    /// <summary>
    /// 验证最小日志级别可解析
    /// </summary>
    /// <param name="configured">配置文本</param>
    /// <param name="expected">期望级别</param>
    [Theory]
    [InlineData("Warning", LogLevel.Warning)]
    [InlineData("Trace", LogLevel.Trace)]
    [InlineData("None", LogLevel.None)]
    public void MinLevel_IsParsed(string configured, LogLevel expected)
    {
        var options = new FileLoggerOptions(TestConfigurationFactory.Create(
            ($"{nameof(FileLoggerOptions)}:{nameof(FileLoggerOptions.MinLevel)}", configured)
        ));

        Assert.Equal(expected, options.MinLevel);
    }

    /// <summary>
    /// 验证最小日志级别配置为非法文本时回退到默认级别 Information
    /// </summary>
    [Fact]
    public void MinLevel_InvalidText_FallsBackToDefault()
    {
        var options = new FileLoggerOptions(TestConfigurationFactory.Create(
            ($"{nameof(FileLoggerOptions)}:{nameof(FileLoggerOptions.MinLevel)}", "Verbose")
        ));

        Assert.Equal(LogLevel.Information, options.MinLevel);
    }

    /// <summary>
    /// 验证属性的 setter 在值未改变时不会重写日志设置文件, 在值改变时会写入新值
    /// </summary>
    [Fact]
    public void Setters_OnlyWriteToFileOnChange()
    {
        var options = new FileLoggerOptions(TestConfigurationFactory.Create())
        {
            MaxLogFileCount = 8
        };
        Assert.True(File.Exists(LogSettingsFilePath));

        const string Sentinel = "SENTINEL-NOT-A-JSON";
        File.WriteAllText(LogSettingsFilePath, Sentinel);

        options.MaxLogFileCount = 8;
        Assert.Equal(Sentinel, File.ReadAllText(LogSettingsFilePath));

        options.MaxLogFileCount = 9;
        var root = JsonNode.Parse(File.ReadAllText(LogSettingsFilePath));
        Assert.Equal(
            9, root?[nameof(FileLoggerOptions)]?[nameof(FileLoggerOptions.MaxLogFileCount)]?.GetValue<int>()
        );
    }
}
