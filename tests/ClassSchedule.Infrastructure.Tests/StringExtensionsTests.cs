using System.Text;

namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// <see cref="StringExtensions"/> 的单元测试
/// </summary>
public sealed class StringExtensionsTests : IDisposable
{
    /// <summary>
    /// UTF-8 编码, 不带 BOM, 用于测试文件读写
    /// </summary>
    private static readonly UTF8Encoding Utf8NoBom = new(false);

    /// <summary>
    /// 测试用临时文件路径
    /// </summary>
    private readonly string _filePath = Path.Combine(
        TestEnvironmentFixture.TempDirectory, $"{Guid.NewGuid():N}.log"
    );

    /// <inheritdoc/>
    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }

    /// <summary>
    /// 验证 <see cref="StringExtensions.OpenAppend"/> 在文件不存在时创建文件并写入内容, 再次打开追加内容
    /// </summary>
    [Fact]
    public void OpenAppend_CreatesFile_AndAppends()
    {
        using (var stream = _filePath.OpenAppend())
        {
            using var writer = new StreamWriter(stream, Utf8NoBom);
            writer.Write("First");
        }

        using (var stream = _filePath.OpenAppend())
        {
            using var writer = new StreamWriter(stream, Utf8NoBom);
            writer.Write("Second");
        }

        Assert.Equal("FirstSecond", File.ReadAllText(_filePath, Utf8NoBom));
    }

    /// <summary>
    /// 验证 <see cref="StringExtensions.OpenAppend"/> 以 <see cref="FileShare.Read"/>
    /// 共享打开, 其他读取流可同时读取
    /// </summary>
    [Fact]
    public void OpenAppend_AllowsConcurrentRead()
    {
        File.WriteAllText(_filePath, "content", Utf8NoBom);
        using var appendStream = _filePath.OpenAppend();
        using var readStream = _filePath.OpenRead();

        using var reader = new StreamReader(readStream, Utf8NoBom);
        Assert.Equal("content", reader.ReadToEnd());
    }

    /// <summary>
    /// 验证 <see cref="StringExtensions.OpenRead"/> 可读取既有文件内容, 且以
    /// <see cref="FileShare.ReadWrite"/> 共享, 多个读取流可同时打开
    /// </summary>
    [Fact]
    public void OpenRead_ReadsContent_AndAllowsConcurrentReaders()
    {
        File.WriteAllText(_filePath, "readable", Utf8NoBom);
        using var first = _filePath.OpenRead();
        using var second = _filePath.OpenRead();

        using var reader = new StreamReader(second, Utf8NoBom);
        Assert.Equal("readable", reader.ReadToEnd());
        Assert.True(first.CanRead);
    }
}
