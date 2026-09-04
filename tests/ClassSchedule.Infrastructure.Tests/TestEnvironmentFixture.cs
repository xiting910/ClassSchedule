namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// 测试环境夹具
/// </summary>
public sealed class TestEnvironmentFixture : IDisposable
{
    /// <summary>
    /// 测试使用的临时目录
    /// </summary>
    public static string TempDirectory { get; } = Path.Combine(Path.GetTempPath(), nameof(ClassSchedule));

    /// <summary>
    /// 测试使用的临时数据根目录
    /// </summary>
    private readonly DirectoryInfo _rootDirectory;

    /// <summary>
    /// 构造函数, 创建临时目录并设置环境变量
    /// </summary>
    public TestEnvironmentFixture()
    {
        var rootDirectory = Path.Combine(TempDirectory, $"{Guid.NewGuid():N}");
        _rootDirectory = Directory.CreateDirectory(rootDirectory);
        Environment.SetEnvironmentVariable(FileSystem.AppDataRootDirVariable, rootDirectory);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Environment.SetEnvironmentVariable(FileSystem.AppDataRootDirVariable, null);
        _rootDirectory.Delete(true);
    }
}
