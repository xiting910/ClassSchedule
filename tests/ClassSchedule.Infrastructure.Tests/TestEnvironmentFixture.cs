using ClassSchedule.Infrastructure.Interfaces;
using ClassSchedule.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// 测试环境夹具, 通过基础设施层的真实注册构建服务容器与真实 SQLite 数据库
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
    /// 服务容器
    /// </summary>
    private readonly ServiceProvider _serviceProvider;

    /// <summary>
    /// 构造函数, 创建临时目录, 设置环境变量并构建服务容器
    /// </summary>
    public TestEnvironmentFixture()
    {
        var rootDirectory = Path.Combine(TempDirectory, $"{Guid.NewGuid():N}");
        _rootDirectory = Directory.CreateDirectory(rootDirectory);
        Environment.SetEnvironmentVariable(FileSystem.AppDataRootDirVariable, rootDirectory);

        var serviceCollection = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddDbContext<AppDbContext>(options => options.UseSqlite(AppDbContext.ConnectionString))
            .AddInfrastructure();

        _serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
        _serviceProvider.GetRequiredService<IDatabaseInitializer>().Initialize();
    }

    /// <summary>
    /// 创建服务范围
    /// </summary>
    /// <returns>服务范围</returns>
    public IServiceScope CreateScope()
    {
        return _serviceProvider.CreateScope();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _serviceProvider.Dispose();
        SqliteConnection.ClearAllPools();
        Environment.SetEnvironmentVariable(FileSystem.AppDataRootDirVariable, null);
        _rootDirectory.Delete(true);
    }
}
