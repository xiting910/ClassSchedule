using Microsoft.Extensions.Configuration;
using System.IO.Compression;
using System.Text.Json;

namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// <see cref="FileSystem"/> 的单元测试
/// </summary>
public sealed class FileSystemTests
{
    /// <summary>
    /// 清理日志目录与设置目录中的全部测试文件
    /// </summary>
    private static void CleanTestFiles()
    {
        foreach (var file in FileSystem.Logs.EnumerateFiles())
        {
            file.Delete();
        }

        foreach (var file in FileSystem.Settings.EnumerateFiles())
        {
            file.Delete();
        }
    }

    /// <summary>
    /// 验证静态成员就绪: 目录树已创建且 JSON 序列化选项符合预期
    /// </summary>
    [Fact]
    public void StaticMembers_DirectoriesCreated_AndJsonOptionsConfigured()
    {
        // 确保 AppDataRootDirectory 的父目录为测试使用的临时目录, 以避免误删除用户数据
        Assert.Equal(TestEnvironmentFixture.TempDirectory, FileSystem.AppDataRootDirectory.Parent?.FullName);

        Assert.True(FileSystem.AppDataRootDirectory.Exists);
        Assert.True(FileSystem.Datas.Exists);
        Assert.True(FileSystem.Logs.Exists);
        Assert.True(FileSystem.Settings.Exists);

        Assert.StartsWith(FileSystem.AppDataRootDirectory.FullName, FileSystem.Datas.FullName);
        Assert.StartsWith(FileSystem.AppDataRootDirectory.FullName, FileSystem.Logs.FullName);
        Assert.StartsWith(FileSystem.AppDataRootDirectory.FullName, FileSystem.Settings.FullName);

        var options = FileSystem.JsonSerializerOptions;
        Assert.True(options.WriteIndented);
        Assert.True(options.AllowTrailingCommas);
        Assert.Equal(JsonCommentHandling.Skip, options.ReadCommentHandling);
    }

    /// <summary>
    /// 验证字符串比较器与操作系统匹配: Windows 为不区分大小写, 其他系统区分大小写
    /// </summary>
    [Fact]
    public void Comparer_MatchesPlatform()
    {
        var expected = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        Assert.Same(expected, FileSystem.Comparer);
    }

    /// <summary>
    /// 验证 <see cref="FileSystem.WriteToFile"/> 在目录不存在时创建目录并写入文件
    /// </summary>
    [Fact]
    public void WriteToFile_CreatesMissingDirectory_AndWritesFile()
    {
        var subDirectoryName = Guid.NewGuid().ToString("N");
        var directory = new DirectoryInfo(Path.Combine(FileSystem.Settings.FullName, subDirectoryName));

        try
        {
            FileSystem.WriteToFile(directory, "test.json", "{\"key\":\"value\"}");

            var writtenPath = Path.Combine(directory.FullName, "test.json");
            Assert.True(File.Exists(writtenPath));
            Assert.Equal("{\"key\":\"value\"}", File.ReadAllText(writtenPath));
        }
        finally
        {
            if (directory.Exists)
            {
                directory.Delete(recursive: true);
            }
        }
    }

    /// <summary>
    /// 验证 <see cref="FileSystem.ExportLogsAsync"/> 在日志目录中无日志文件时产出空压缩包且不抛异常
    /// </summary>
    [Fact]
    public async Task ExportLogsAsync_NoLogFiles_WritesEmptyArchive()
    {
        CleanTestFiles();
        await using var stream = new MemoryStream();

        await FileSystem.ExportLogsAsync(stream, TestContext.Current.CancellationToken);

        stream.Position = 0;
        await using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
        Assert.Empty(archive.Entries);
    }

    /// <summary>
    /// 验证 <see cref="FileSystem.ExportLogsAsync"/> 将日志目录下全部日志文件打包, 条目名与内容完整
    /// </summary>
    [Fact]
    public async Task ExportLogsAsync_PacksAllLogFilesWithContent()
    {
        CleanTestFiles();
        var logs = FileSystem.Logs;
        File.WriteAllText(Path.Combine(logs.FullName, "a.log"), "alpha");
        File.WriteAllText(Path.Combine(logs.FullName, "b.log"), "beta");
        await using var stream = new MemoryStream();

        await FileSystem.ExportLogsAsync(stream, TestContext.Current.CancellationToken);

        stream.Position = 0;
        await using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
        Assert.Equal(2, archive.Entries.Count);
        Assert.Equal(
            ["a.log", "b.log"],
            archive.Entries.Select(entry => entry.Name).Order(StringComparer.Ordinal)
        );

        var content = new Dictionary<string, string>();
        foreach (var entry in archive.Entries)
        {
            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            content[entry.Name] = reader.ReadToEnd();
        }

        Assert.Equal("alpha", content["a.log"]);
        Assert.Equal("beta", content["b.log"]);
    }

    /// <summary>
    /// 验证 <see cref="FileSystem.AddJsonFilesFromSettings"/> 将设置目录下全部 JSON 文件加入配置
    /// </summary>
    [Fact]
    public void AddJsonFilesFromSettings_LoadsAllJsonFiles()
    {
        CleanTestFiles();
        var settings = FileSystem.Settings;
        FileSystem.WriteToFile(settings, "settings-a.json", "{\"First\":\"value-a\"}");
        FileSystem.WriteToFile(settings, "settings-b.json", "{\"Second\":42}");

        var builder = new ConfigurationBuilder();
        var config = builder.AddJsonFilesFromSettings().Build();

        Assert.Equal("value-a", config["First"]);
        Assert.Equal("42", config["Second"]);
    }
}
