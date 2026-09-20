using ClassSchedule.Domain.Models;
using ClassSchedule.UI.Shared.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="NavigationStack"/> 的单元测试
/// </summary>
public sealed class NavigationStackTests
{
    /// <summary>
    /// 页面载入失败时的错误信息
    /// </summary>
    private const string FailureMessage = "载入失败";

    /// <summary>
    /// 页面载入抛出异常时的异常信息
    /// </summary>
    private const string ExceptionMessage = "载入时发生异常";

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
            .AddSingleton<ToastViewModel>()
            .AddSingleton<NavigationStack>();

        configure?.Invoke(services);
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    /// <summary>
    /// 验证载入成功时页面入栈
    /// </summary>
    [Fact]
    public async Task PushAsync_载入成功_页面入栈()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped<FakePageViewModel>());
            var navigationStack = provider.GetRequiredService<NavigationStack>();

            await navigationStack.PushAsync<FakePageViewModel>();

            var page = Assert.IsType<FakePageViewModel>(navigationStack.CurrentPage);
            Assert.True(navigationStack.HasPage);
            Assert.False(page.IsDisposed);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证载入失败时弹出提示且页面不入栈
    /// </summary>
    [Fact]
    public async Task PushAsync_载入失败_弹出提示且不入栈()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped(_ => new FakePageViewModel
            {
                LoadResult = Result.Failure(ErrorCode.TimetableNotFound, FailureMessage)
            }));
            var navigationStack = provider.GetRequiredService<NavigationStack>();
            var toast = provider.GetRequiredService<ToastViewModel>();

            await navigationStack.PushAsync<FakePageViewModel>();

            Assert.False(navigationStack.HasPage);
            Assert.Null(navigationStack.CurrentPage);
            Assert.Equal(FailureMessage, Assert.Single(toast.Items).Message);

            toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证载入失败时释放该页的服务范围, 连带释放容器创建的页面视图模型
    /// </summary>
    [Fact]
    public async Task PushAsync_载入失败_释放页面范围()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            FakePageViewModel loaded = null!;
            using var provider = CreateProvider(services => services.AddScoped(_ =>
            {
                var page = new FakePageViewModel
                {
                    LoadResult = Result.Failure(ErrorCode.TimetableNotFound, FailureMessage)
                };
                loaded = page;

                return page;
            }));
            var navigationStack = provider.GetRequiredService<NavigationStack>();

            await navigationStack.PushAsync<FakePageViewModel>();

            Assert.True(loaded.IsDisposed);

            provider.GetRequiredService<ToastViewModel>().Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证载入抛出异常时不向外传播, 弹出提示并释放该页的服务范围
    /// </summary>
    [Fact]
    public async Task PushAsync_载入抛异常_弹出提示且释放页面范围()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            FakePageViewModel loaded = null!;
            using var provider = CreateProvider(services => services.AddScoped(_ =>
            {
                var page = new FakePageViewModel
                {
                    LoadException = new InvalidOperationException(ExceptionMessage)
                };
                loaded = page;

                return page;
            }));
            var navigationStack = provider.GetRequiredService<NavigationStack>();
            var toast = provider.GetRequiredService<ToastViewModel>();

            await navigationStack.PushAsync<FakePageViewModel>();

            Assert.False(navigationStack.HasPage);
            Assert.Null(navigationStack.CurrentPage);
            Assert.True(loaded.IsDisposed);
            Assert.Contains(ExceptionMessage, Assert.Single(toast.Items).Message);

            toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证栈内已有页面时, 一次失败的入栈不影响原有页面
    /// </summary>
    [Fact]
    public async Task PushAsync_载入失败_保留已有页面()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var loadResult = Result.Success();
            using var provider = CreateProvider(services => services.AddScoped(
                _ => new FakePageViewModel { LoadResult = loadResult }
            ));
            var navigationStack = provider.GetRequiredService<NavigationStack>();

            await navigationStack.PushAsync<FakePageViewModel>();
            var first = Assert.IsType<FakePageViewModel>(navigationStack.CurrentPage);

            loadResult = Result.Failure(ErrorCode.TimetableNotFound, FailureMessage);
            await navigationStack.PushAsync<FakePageViewModel>();

            Assert.True(navigationStack.HasPage);
            Assert.Same(first, navigationStack.CurrentPage);

            provider.GetRequiredService<ToastViewModel>().Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证连续入栈两次时, 每页都在各自的服务范围里解析出独立的视图模型
    /// </summary>
    [Fact]
    public async Task PushAsync_连续两次_每页各自独立范围()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped<FakePageViewModel>());
            var navigationStack = provider.GetRequiredService<NavigationStack>();

            await navigationStack.PushAsync<FakePageViewModel>();
            var first = Assert.IsType<FakePageViewModel>(navigationStack.CurrentPage);

            await navigationStack.PushAsync<FakePageViewModel>();
            var second = Assert.IsType<FakePageViewModel>(navigationStack.CurrentPage);

            Assert.NotSame(first, second);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证带载入参数的入栈把参数交给页面视图模型
    /// </summary>
    [Fact]
    public async Task PushAsync_带载入参数_参数传给页面视图模型()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped<FakePageViewModel>());
            var navigationStack = provider.GetRequiredService<NavigationStack>();
            var argument = Guid.NewGuid();

            await navigationStack.PushAsync<FakePageViewModel, Guid>(argument);

            Assert.True(navigationStack.HasPage);
            var page = Assert.IsType<FakePageViewModel>(navigationStack.CurrentPage);
            Assert.Equal(argument, page.LoadArgument);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证带载入参数的入栈在载入失败时同样弹出提示且不入栈
    /// </summary>
    [Fact]
    public async Task PushAsync_带载入参数载入失败_弹出提示且不入栈()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped(_ => new FakePageViewModel
            {
                LoadResult = Result.Failure(ErrorCode.TimetableNotFound, FailureMessage)
            }));
            var navigationStack = provider.GetRequiredService<NavigationStack>();
            var toast = provider.GetRequiredService<ToastViewModel>();

            await navigationStack.PushAsync<FakePageViewModel, Guid>(Guid.NewGuid());

            Assert.False(navigationStack.HasPage);
            Assert.Null(navigationStack.CurrentPage);
            Assert.Equal(FailureMessage, Assert.Single(toast.Items).Message);

            toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证栈为空时弹出请求不产生任何变化
    /// </summary>
    [Fact]
    public async Task TryPop_栈为空_返回false()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider();
            var navigationStack = provider.GetRequiredService<NavigationStack>();

            Assert.False(navigationStack.TryPop());
            Assert.False(navigationStack.HasPage);
            Assert.Null(navigationStack.CurrentPage);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证弹出页面时只释放被弹出页面的服务范围
    /// </summary>
    [Fact]
    public async Task TryPop_弹出一层_只释放被弹出页面的范围()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped<FakePageViewModel>());
            var navigationStack = provider.GetRequiredService<NavigationStack>();

            await navigationStack.PushAsync<FakePageViewModel>();
            var first = Assert.IsType<FakePageViewModel>(navigationStack.CurrentPage);

            await navigationStack.PushAsync<FakePageViewModel>();
            var second = Assert.IsType<FakePageViewModel>(navigationStack.CurrentPage);

            Assert.True(navigationStack.TryPop());
            Assert.True(second.IsDisposed);
            Assert.False(first.IsDisposed);
            Assert.Same(first, navigationStack.CurrentPage);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证弹出最后一页时清空当前页面并消费本次弹出
    /// </summary>
    [Fact]
    public async Task TryPop_弹出最后一页_清空当前页面并返回true()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped<FakePageViewModel>());
            var navigationStack = provider.GetRequiredService<NavigationStack>();

            await navigationStack.PushAsync<FakePageViewModel>();
            var page = Assert.IsType<FakePageViewModel>(navigationStack.CurrentPage);

            Assert.True(navigationStack.TryPop());
            Assert.False(navigationStack.HasPage);
            Assert.Null(navigationStack.CurrentPage);
            Assert.True(page.IsDisposed);

            return 0;
        }, TestContext.Current.CancellationToken);
    }
}
