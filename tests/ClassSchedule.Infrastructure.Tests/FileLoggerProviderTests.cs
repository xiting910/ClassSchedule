using ClassSchedule.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// <see cref="FileLoggerProvider"/> 的单元测试
/// </summary>
public sealed partial class FileLoggerProviderTests
{
    /// <summary>
    /// 最新日志文件名
    /// </summary>
    private const string LatestLogFileName = $"Latest{FileSystem.LogFileSuffix}";

    /// <summary>
    /// 最新日志文件完整路径
    /// </summary>
    private static string LatestLogFilePath => Path.Combine(FileSystem.Logs.FullName, LatestLogFileName);

    /// <summary>
    /// 日志时间戳前缀的正则表达式
    /// </summary>
    [GeneratedRegex(@"^\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] ", RegexOptions.CultureInvariant)]
    private static partial Regex TimestampPrefixRegex();

    /// <summary>
    /// 删除日志目录中的全部日志文件
    /// </summary>
    private static void CleanLogFiles()
    {
        foreach (var file in FileSystem.Logs.EnumerateFiles())
        {
            file.Delete();
        }
    }

    /// <summary>
    /// 验证日志以时间戳/级别/类别/消息格式写入 Latest.log, 且 Dispose 后已完成刷新
    /// </summary>
    [Fact]
    public void Logger_WritesFormattedLineToLatestLog()
    {
        CleanLogFiles();
        using var provider = new FileLoggerProvider(
            TimeProvider.System, new(TestConfigurationFactory.Create())
        );
        var logger = provider.CreateLogger("ClassSchedule.Tests.Category");

        logger.LogInformation("Hello {Name}", "world");
        provider.Dispose();

        Assert.True(File.Exists(LatestLogFilePath));
        var content = File.ReadAllText(LatestLogFilePath);
        Assert.Matches(TimestampPrefixRegex(), content);
        Assert.Contains("[Information]", content);
        Assert.Contains("[ClassSchedule.Tests.Category]", content);
        Assert.Contains("Hello world", content);
    }

    /// <summary>
    /// 验证不同类别与级别的日志都会落盘, 异常信息包含在日志行中
    /// </summary>
    [Fact]
    public void Logger_WritesMultipleCategoriesLevelsAndException()
    {
        CleanLogFiles();
        using var provider = new FileLoggerProvider(
            TimeProvider.System, new(TestConfigurationFactory.Create())
        );
        var firstLogger = provider.CreateLogger("Category.A");
        var secondLogger = provider.CreateLogger("Category.B");

        firstLogger.LogWarning("warn {Value}", 1);
        secondLogger.LogError(new InvalidOperationException("boom"), "fail {Value}", 2);
        provider.Dispose();

        var content = File.ReadAllText(LatestLogFilePath);
        Assert.Contains("[Warning]", content);
        Assert.Contains("[Category.A]", content);
        Assert.Contains("warn 1", content);
        Assert.Contains("[Error]", content);
        Assert.Contains("[Category.B]", content);
        Assert.Contains("fail 2", content);
        Assert.Contains(nameof(InvalidOperationException), content);
        Assert.Contains("boom", content);
    }

    /// <summary>
    /// 验证低于最小日志级别的日志不会写入文件
    /// </summary>
    [Fact]
    public void Logger_BelowMinLevel_IsNotWritten()
    {
        CleanLogFiles();
        using var provider = new FileLoggerProvider(TimeProvider.System, new(TestConfigurationFactory.Create(
            ($"{nameof(FileLoggerOptions)}:{nameof(FileLoggerOptions.MinLevel)}", nameof(LogLevel.Warning))
        )));
        var logger = provider.CreateLogger("ClassSchedule.Tests.Category");

        logger.LogInformation("should not appear");
        provider.Dispose();

        Assert.False(File.Exists(LatestLogFilePath));
    }

    /// <summary>
    /// 验证轮转: 构造提供器时非空的 Latest.log 被移动为按时间命名的文件,
    /// 且超出保留数量的旧文件被删除 (保留数量为 MaxLogFileCount - 1)
    /// </summary>
    [Fact]
    public void Rotate_MovesNonEmptyLatestAndPrunesOldFiles()
    {
        CleanLogFiles();
        File.WriteAllText(LatestLogFilePath, "previous session logs");
        for (var i = 1; i <= 5; i++)
        {
            File.WriteAllText(Path.Combine(
                FileSystem.Logs.FullName, $"2000-01-01_00000{i}{FileSystem.LogFileSuffix}"
            ), "old");
        }

        using var provider = new FileLoggerProvider(TimeProvider.System, new(TestConfigurationFactory.Create(
            ($"{nameof(FileLoggerOptions)}:{nameof(FileLoggerOptions.MaxLogFileCount)}", "1")
        )));

        var remaining = FileSystem.Logs.EnumerateFiles($"*{FileSystem.LogFileSuffix}");
        Assert.Empty(remaining);
    }

    /// <summary>
    /// 验证轮转: 构造提供器后仅保留按名字降序最新的 (MaxLogFileCount - 1) 个日志文件 (含移动得到的 now 文件),
    /// 其余旧文件被删除
    /// </summary>
    [Fact]
    public void Rotate_KeepsNewestFilesAndDeletesOldest()
    {
        CleanLogFiles();
        File.WriteAllText(LatestLogFilePath, "previous session logs");
        for (var i = 1; i <= 5; i++)
        {
            File.WriteAllText(Path.Combine(
                FileSystem.Logs.FullName, $"2000-01-01_00000{i}{FileSystem.LogFileSuffix}"
            ), "old");
        }

        using var provider = new FileLoggerProvider(TimeProvider.System, new(TestConfigurationFactory.Create(
            ($"{nameof(FileLoggerOptions)}:{nameof(FileLoggerOptions.MaxLogFileCount)}", "3")
        )));

        var remaining = FileSystem.Logs
            .EnumerateFiles($"*{FileSystem.LogFileSuffix}")
            .Select(file => file.Name)
            .ToArray();

        Assert.Equal(2, remaining.Length);
        Assert.DoesNotContain(LatestLogFileName, remaining);
        Assert.Contains($"2000-01-01_000005{FileSystem.LogFileSuffix}", remaining);
        Assert.DoesNotContain($"2000-01-01_000004{FileSystem.LogFileSuffix}", remaining);
        Assert.DoesNotContain($"2000-01-01_000003{FileSystem.LogFileSuffix}", remaining);
        Assert.DoesNotContain($"2000-01-01_000002{FileSystem.LogFileSuffix}", remaining);
        Assert.DoesNotContain($"2000-01-01_000001{FileSystem.LogFileSuffix}", remaining);
    }

    /// <summary>
    /// 验证轮转: 空的 Latest.log 不会被移动 (文件名保持不变)
    /// </summary>
    [Fact]
    public void Rotate_EmptyLatestLog_IsNotMoved()
    {
        CleanLogFiles();
        File.WriteAllText(LatestLogFilePath, string.Empty);
        File.WriteAllText(
            Path.Combine(FileSystem.Logs.FullName, $"2000-01-01_000001{FileSystem.LogFileSuffix}"), "old"
        );

        using var provider = new FileLoggerProvider(TimeProvider.System, new(
            TestConfigurationFactory.Create((nameof(FileLoggerOptions.MaxLogFileCount), "5"))
        ));

        Assert.True(File.Exists(LatestLogFilePath));
        Assert.Equal(0, new FileInfo(LatestLogFilePath).Length);
        Assert.Equal(2, FileSystem.Logs.EnumerateFiles($"*{FileSystem.LogFileSuffix}").Count());
    }
}
