using Avalonia.Controls.ApplicationLifetimes;
using ClassSchedule.Infrastructure;
using ClassSchedule.UI.Shared.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// 组合根 (composition root) 测试: 按桌面/安卓入口相同的注册流程组装三层服务,
/// 验证应用启动时所需服务均可解析且单例注册生效, 防止注册缺失导致的运行时崩溃
/// </summary>
public sealed class CompositionRootTests
{
    /// <summary>
    /// 测试用的壳初始化器桩
    /// </summary>
    private sealed class StubShellInitializer : IShellInitializer
    {
        /// <inheritdoc/>
        public void Initialize(IApplicationLifetime? lifetime) { }
    }

    /// <summary>
    /// 按入口注册流程构建服务集合
    /// </summary>
    /// <returns>服务集合</returns>
    private static ServiceProvider CreateServiceCollection()
    {
        return new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddLogging(builder => builder.AddFileLogger())
            .AddInfrastructure()
            .AddUIShared()
            .AddSingleton<IShellInitializer, StubShellInitializer>()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
    }

    /// <summary>
    /// 验证完整注册流程下服务图可构建, 启动所需服务均可解析, 关键服务为单例
    /// </summary>
    [Fact]
    public async Task 组合根_三层注册_启动所需服务均可解析且为单例()
    {
        // ToastViewModel 构造会创建 UI 计时器, 需在 headless UI 线程上解析
        await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            using var serviceProvider = CreateServiceCollection();

            Assert.NotNull(serviceProvider.GetRequiredService<ToastViewModel>());
            Assert.NotNull(serviceProvider.GetRequiredService<ShellViewModel>());
            Assert.NotNull(serviceProvider.GetRequiredService<UIOptions>());
            Assert.NotNull(serviceProvider.GetRequiredService<IShellInitializer>());
            Assert.NotNull(serviceProvider.GetRequiredService<FileLoggerOptions>());

            Assert.Same(TimeProvider.System, serviceProvider.GetRequiredService<TimeProvider>());

            Assert.Same(
                serviceProvider.GetRequiredService<ToastViewModel>(),
                serviceProvider.GetRequiredService<ToastViewModel>()
            );
            Assert.Same(
                serviceProvider.GetRequiredService<ShellViewModel>(),
                serviceProvider.GetRequiredService<ShellViewModel>()
            );
        }, TestContext.Current.CancellationToken);
    }
}
