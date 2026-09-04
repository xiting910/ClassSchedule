using Avalonia.Headless;
using ClassSchedule.Infrastructure;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// 测试环境夹具
/// </summary>
public sealed class TestEnvironmentFixture : IAsyncDisposable
{
    /// <summary>
    /// 测试使用的临时目录
    /// </summary>
    public static string TempDirectory { get; } = Path.Combine(Path.GetTempPath(), nameof(ClassSchedule));

    /// <summary>
    /// headless 会话实例, 在所有测试运行前启动, 在所有测试运行后释放
    /// </summary>
    public static HeadlessUnitTestSession Session
    {
        get => field ?? throw new InvalidOperationException($"{nameof(Session)} has not been initialized.");
        private set;
    }

    /// <summary>
    /// 测试使用的临时数据根目录
    /// </summary>
    private readonly DirectoryInfo _rootDirectory;

    /// <summary>
    /// 构造函数, 创建临时目录, 设置环境变量并启动 headless 会话
    /// </summary>
    public TestEnvironmentFixture()
    {
        var rootDirectory = Path.Combine(TempDirectory, $"{Guid.NewGuid():N}");
        _rootDirectory = Directory.CreateDirectory(rootDirectory);
        Environment.SetEnvironmentVariable(FileSystem.AppDataRootDirVariable, rootDirectory);
        Session = HeadlessUnitTestSession.StartNew(typeof(TestApplication));
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await Session.DisposeAsync();
        Environment.SetEnvironmentVariable(FileSystem.AppDataRootDirVariable, null);
        _rootDirectory.Delete(true);
    }
}
