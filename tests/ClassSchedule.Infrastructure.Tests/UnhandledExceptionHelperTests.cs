using System.Text;
using System.Text.RegularExpressions;

namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// <see cref="UnhandledExceptionHelper"/> 的单元测试
/// </summary>
public sealed partial class UnhandledExceptionHelperTests
{
    /// <summary>
    /// 未处理异常日志时间戳前缀的正则表达式
    /// </summary>
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} ", RegexOptions.CultureInvariant)]
    private static partial Regex TimestampPrefixRegex();

    /// <summary>
    /// 删除可能存在的未处理异常日志文件
    /// </summary>
    private static void DeleteLogFile()
    {
        if (File.Exists(UnhandledExceptionHelper.UnhandledExceptionLogFilePath))
        {
            File.Delete(UnhandledExceptionHelper.UnhandledExceptionLogFilePath);
        }
    }

    /// <summary>
    /// 验证 <see cref="UnhandledExceptionHelper.HandleException"/> 写入包含时间戳/终止标记/异常文本的日志条目
    /// </summary>
    [Fact]
    public void HandleException_WritesEntryWithTimestampAndMarker()
    {
        DeleteLogFile();
        var marker = Guid.NewGuid().ToString("N");

        UnhandledExceptionHelper.HandleException(true, new InvalidOperationException(marker));

        Assert.True(File.Exists(UnhandledExceptionHelper.UnhandledExceptionLogFilePath));
        var content = File.ReadAllText(UnhandledExceptionHelper.UnhandledExceptionLogFilePath);
        Assert.Matches(TimestampPrefixRegex(), content);
        Assert.Contains("[Terminating]", content);
        Assert.Contains(nameof(InvalidOperationException), content);
        Assert.Contains(marker, content);
    }

    /// <summary>
    /// 验证连续调用 <see cref="UnhandledExceptionHelper.HandleException"/> 时日志条目按顺序追加, 不覆盖既有条目
    /// </summary>
    [Fact]
    public void HandleException_AppendsEntriesInOrder()
    {
        DeleteLogFile();
        var firstMarker = Guid.NewGuid().ToString("N");
        var secondMarker = Guid.NewGuid().ToString("N");

        UnhandledExceptionHelper.HandleException(true, new InvalidOperationException(firstMarker));
        UnhandledExceptionHelper.HandleException(false, new InvalidOperationException(secondMarker));

        var content = File.ReadAllText(UnhandledExceptionHelper.UnhandledExceptionLogFilePath);
        Assert.Contains("[Terminating]", content);
        Assert.Contains("[Non-Terminating]", content);
        Assert.True(
            content.IndexOf(firstMarker, StringComparison.Ordinal) <
            content.IndexOf(secondMarker, StringComparison.Ordinal)
        );
    }

    /// <summary>
    /// 验证 <see cref="UnhandledExceptionHelper.ExportLogAsync"/> 将日志文件内容完整复制到目标流
    /// </summary>
    [Fact]
    public async Task ExportLogAsync_CopiesFileContent()
    {
        DeleteLogFile();
        UnhandledExceptionHelper.HandleException(false, new InvalidOperationException("export-me"));
        await using var stream = new MemoryStream();

        await UnhandledExceptionHelper.ExportLogAsync(stream, TestContext.Current.CancellationToken);

        var expected = File.ReadAllText(UnhandledExceptionHelper.UnhandledExceptionLogFilePath);
        Assert.Equal(expected, new UTF8Encoding(false).GetString(stream.ToArray()));
    }

    /// <summary>
    /// 验证 <see cref="UnhandledExceptionHelper.ExportLogAsync"/> 在日志文件不存在时不抛异常且输出为空
    /// </summary>
    [Fact]
    public async Task ExportLogAsync_NoLogFile_ProducesEmptyOutput()
    {
        DeleteLogFile();
        await using var stream = new MemoryStream();

        await UnhandledExceptionHelper.ExportLogAsync(stream, TestContext.Current.CancellationToken);

        Assert.Equal(0, stream.Length);
    }
}
