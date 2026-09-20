using Avalonia;
using Avalonia.Styling;
using ClassSchedule.Infrastructure;
using ClassSchedule.Infrastructure.Interfaces;
using ClassSchedule.UI.Shared.Models;
using ClassSchedule.UI.Shared.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ClassSchedule.UI.Shared;

/// <summary>
/// 应用程序类
/// </summary>
public sealed partial class App : Application
{
    /// <summary>
    /// 服务容器, 由平台入口在启动时注入
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceProvider Services
    {
        get => field ?? throw new InvalidOperationException($"{nameof(Services)} is not initialized.");
        private set;
    }

    /// <summary>
    /// 创建服务容器, 由平台入口在启动时调用, 以便在应用程序启动前注册依赖注入服务
    /// </summary>
    /// <typeparam name="T">应用的核心壳视图的初始化器类型</typeparam>
    /// <returns>创建的服务容器</returns>
    public static ServiceProvider CreateServices<T>() where T : class, IShellInitializer
    {
        var configurationBuilder = new ConfigurationBuilder();
        var config = configurationBuilder.AddJsonFilesFromSettings().Build();

        var serviceCollection = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddLogging(builder => builder.AddFileLogger())
            .AddInfrastructure()
            .AddUIShared()
            .AddSingleton<IShellInitializer, T>();

        var provider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        Services = provider;
        return provider;
    }

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        // 调用基类方法, 以便在应用程序启动时完成框架初始化
        base.OnFrameworkInitializationCompleted();

        // 获取服务容器
        var services = Services;

        // 获取提示视图模型, 以便在未处理异常时显示提示
        var toastViewModel = services.GetRequiredService<ToastViewModel>();

        // 处理未处理的 UI 线程异常, 显示提示并写入日志文件
        Avalonia.Threading.Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            var ex = e.Exception;
            UnhandledExceptionHelper.HandleException(false, ex);
            toastViewModel.Show(
                $"发生未处理的 UI 线程异常: {ex.Message}, 阅读 " +
                UnhandledExceptionHelper.UnhandledExceptionLogFilePath +
                " 以查看详细信息"
            );
            e.Handled = true;
        };

        // 处理未处理的任务异常, 显示提示并写入日志文件
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            var ex = e.Exception;
            UnhandledExceptionHelper.HandleException(false, ex);
            Avalonia.Threading.Dispatcher.UIThread.Post(() => toastViewModel.Show(
                $"发生未处理的任务异常: {ex.Message}, 阅读 " +
                UnhandledExceptionHelper.UnhandledExceptionLogFilePath +
                " 以查看详细信息"
            ));
            e.SetObserved();
        };

        // 按配置的主题模式应用主题
        Current?.RequestedThemeVariant = services.GetRequiredService<UIOptions>().Theme switch
        {
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };

        // 初始化应用的核心壳视图
        services.GetRequiredService<IShellInitializer>().Initialize(ApplicationLifetime);

        // 初始化数据库
        services.GetRequiredService<IDatabaseInitializer>().Initialize();

        // 按首页决策打开启动页
        services.GetRequiredService<ShellViewModel>().OpenHomePage();
    }
}
