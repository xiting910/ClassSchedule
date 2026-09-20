using ClassSchedule.Infrastructure.Interfaces;
using ClassSchedule.UI.Shared.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="ShellViewModel"/> 的单元测试
/// </summary>
public sealed class ShellViewModelTests
{
    /// <summary>
    /// 创建服务容器
    /// </summary>
    /// <param name="configure">追加服务注册的委托, 页面视图模型由各用例按页注册</param>
    /// <returns>服务容器</returns>
    private static ServiceProvider CreateProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddSingleton<UIOptions>()
            .AddSingleton<NavigationStack>()
            .AddSingleton<OverlayHostViewModel>()
            .AddSingleton<ShellViewModel>()
            .AddSingleton<ToastViewModel>();

        configure?.Invoke(services);
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    /// <summary>
    /// 验证未指定当前课表时打开首页会推入课表列表页
    /// </summary>
    [Fact]
    public async Task OpenHomePage_未指定当前课表_推入课表列表页()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = new Mock<ITimetableRepository>();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            using var provider = CreateProvider(services => services
                .AddSingleton(repository.Object)
                .AddScoped<TimetableListViewModel>());
            var shell = provider.GetRequiredService<ShellViewModel>();

            shell.OpenHomePage();
            await Task.Delay(300);

            Assert.True(shell.NavigationStack.HasPage);
            var page = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);
            Assert.True(page.IsEmpty);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证三种返回源同时存在时按优先级逐个消费, 全部消费完之后返回 false 交给系统
    /// </summary>
    [Fact]
    public async Task TryGoBack_三种返回源同时存在_按优先级逐个消费直到返回false()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped<FakePageViewModel>());
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.NavigationStack.PushAsync<FakePageViewModel>();
            var first = Assert.IsType<FakePageViewModel>(shell.NavigationStack.CurrentPage);
            await shell.NavigationStack.PushAsync<FakePageViewModel>();
            var second = Assert.IsType<FakePageViewModel>(shell.NavigationStack.CurrentPage);
            shell.OverlayHost.OpenConfirmOverlay(
                "删除片段", "删除后无法恢复", "删除", () => { }
            );
            shell.SelectedTabIndex = 2;

            // 浮层在最上层, 只关它, 页面与 Tab 都不动
            Assert.True(shell.TryGoBack());
            Assert.False(shell.OverlayHost.HasOverlay);
            Assert.Same(second, shell.NavigationStack.CurrentPage);
            Assert.Equal(2, shell.SelectedTabIndex);

            // 浮层没了才轮到页面, 一次弹一层
            Assert.True(shell.TryGoBack());
            Assert.True(shell.NavigationStack.HasPage);
            Assert.Same(first, shell.NavigationStack.CurrentPage);
            Assert.True(second.IsDisposed);
            Assert.False(first.IsDisposed);
            Assert.Equal(2, shell.SelectedTabIndex);

            Assert.True(shell.TryGoBack());
            Assert.False(shell.NavigationStack.HasPage);
            Assert.Null(shell.NavigationStack.CurrentPage);
            Assert.True(first.IsDisposed);
            Assert.Equal(2, shell.SelectedTabIndex);

            // 页面也没了才轮到 Tab, 切回课表 Tab 而不是退出
            Assert.True(shell.TryGoBack());
            Assert.Equal(0, shell.SelectedTabIndex);

            // 三种来源都空了, 不消费, 交给系统退出
            Assert.False(shell.TryGoBack());

            return 0;
        }, TestContext.Current.CancellationToken);
    }
}
