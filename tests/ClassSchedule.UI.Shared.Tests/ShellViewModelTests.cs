using ClassSchedule.Domain.Models;
using ClassSchedule.UI.Shared.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="ShellViewModel"/> 的单元测试
/// </summary>
public sealed class ShellViewModelTests
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
    /// 验证载入成功时页面入栈, 并同步覆盖层状态
    /// </summary>
    [Fact]
    public async Task PushAsync_载入成功_页面入栈并同步覆盖层状态()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped<FakePageViewModel>());
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.PushAsync<FakePageViewModel>();

            var page = Assert.IsType<FakePageViewModel>(shell.CurrentPage);
            Assert.True(shell.HasPage);
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
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.PushAsync<FakePageViewModel>();

            Assert.False(shell.HasPage);
            Assert.Null(shell.CurrentPage);

            var toast = Assert.Single(shell.Toast.Items);
            Assert.Equal(FailureMessage, toast.Message);

            shell.Toast.Items.Clear();
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
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.PushAsync<FakePageViewModel>();

            Assert.NotNull(loaded);
            Assert.True(loaded.IsDisposed);

            shell.Toast.Items.Clear();
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
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.PushAsync<FakePageViewModel>();

            Assert.False(shell.HasPage);
            Assert.Null(shell.CurrentPage);
            Assert.NotNull(loaded);
            Assert.True(loaded.IsDisposed);
            var toast = Assert.Single(shell.Toast.Items);
            Assert.Contains(ExceptionMessage, toast.Message);

            shell.Toast.Items.Clear();
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
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.PushAsync<FakePageViewModel>();
            var first = Assert.IsType<FakePageViewModel>(shell.CurrentPage);

            loadResult = Result.Failure(ErrorCode.TimetableNotFound, FailureMessage);
            await shell.PushAsync<FakePageViewModel>();

            Assert.True(shell.HasPage);
            Assert.Same(first, shell.CurrentPage);

            shell.Toast.Items.Clear();
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
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.PushAsync<FakePageViewModel>();
            var first = Assert.IsType<FakePageViewModel>(shell.CurrentPage);

            await shell.PushAsync<FakePageViewModel>();
            var second = Assert.IsType<FakePageViewModel>(shell.CurrentPage);

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
            var shell = provider.GetRequiredService<ShellViewModel>();
            var argument = Guid.NewGuid();

            await shell.PushAsync<Guid, FakePageViewModel>(argument);

            Assert.True(shell.HasPage);
            var page = Assert.IsType<FakePageViewModel>(shell.CurrentPage);
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
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.PushAsync<Guid, FakePageViewModel>(Guid.NewGuid());

            Assert.False(shell.HasPage);
            Assert.Null(shell.CurrentPage);
            var toast = Assert.Single(shell.Toast.Items);
            Assert.Equal(FailureMessage, toast.Message);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证栈非空时返回一次弹出一层, 并回到下一层的页面
    /// </summary>
    [Fact]
    public async Task TryGoBack_栈非空_弹出一层并回到上一页()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped<FakePageViewModel>());
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.PushAsync<FakePageViewModel>();
            var first = Assert.IsType<FakePageViewModel>(shell.CurrentPage);

            await shell.PushAsync<FakePageViewModel>();

            Assert.True(shell.TryGoBack());
            Assert.True(shell.HasPage);
            Assert.Same(first, shell.CurrentPage);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证弹出页面时只释放被弹出页面的服务范围
    /// </summary>
    [Fact]
    public async Task TryGoBack_弹出一层_只释放被弹出页面的范围()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped<FakePageViewModel>());
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.PushAsync<FakePageViewModel>();
            var first = Assert.IsType<FakePageViewModel>(shell.CurrentPage);

            await shell.PushAsync<FakePageViewModel>();
            var second = Assert.IsType<FakePageViewModel>(shell.CurrentPage);

            Assert.True(shell.TryGoBack());
            Assert.True(second.IsDisposed);
            Assert.False(first.IsDisposed);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证弹出最后一页时清空当前页面并释放它的服务范围
    /// </summary>
    [Fact]
    public async Task TryGoBack_弹出最后一页_清空当前页面并返回true()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped<FakePageViewModel>());
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.PushAsync<FakePageViewModel>();
            var page = Assert.IsType<FakePageViewModel>(shell.CurrentPage);

            Assert.True(shell.TryGoBack());
            Assert.False(shell.HasPage);
            Assert.Null(shell.CurrentPage);
            Assert.True(page.IsDisposed);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证导航栈为空且不在课表 Tab 时切回课表 Tab 并消费本次返回
    /// </summary>
    [Fact]
    public async Task TryGoBack_栈空且不在课表Tab_切回课表并返回true()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider();
            var shell = provider.GetRequiredService<ShellViewModel>();

            shell.SelectedTabIndex = 2;

            Assert.True(shell.TryGoBack());
            Assert.Equal(0, shell.SelectedTabIndex);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证导航栈为空且在课表 Tab 时不消费本次返回, 交给系统处理
    /// </summary>
    [Fact]
    public async Task TryGoBack_栈空且在课表Tab_返回false()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider();
            var shell = provider.GetRequiredService<ShellViewModel>();

            Assert.False(shell.TryGoBack());
            Assert.Equal(0, shell.SelectedTabIndex);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证导航栈非空时优先弹栈, 不切换底部的 Tab
    /// </summary>
    [Fact]
    public async Task TryGoBack_栈非空且不在课表Tab_优先弹栈()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(services => services.AddScoped<FakePageViewModel>());
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.PushAsync<FakePageViewModel>();
            shell.SelectedTabIndex = 2;

            Assert.True(shell.TryGoBack());
            Assert.False(shell.HasPage);
            Assert.Equal(2, shell.SelectedTabIndex);

            return 0;
        }, TestContext.Current.CancellationToken);
    }
}
