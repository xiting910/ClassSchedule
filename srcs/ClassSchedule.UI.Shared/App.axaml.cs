using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClassSchedule.UI.Shared;

/// <summary>
/// 应用程序类
/// </summary>
public sealed partial class App : Application
{
    /// <summary>
    /// 应用程序退出取消令牌源, 用于在应用程序退出时取消等待的任务
    /// </summary>
    public static CancellationTokenSource ExitCts { get; } = new();

    /// <summary>
    /// 服务容器, 由平台入口在启动时注入
    /// </summary>
    /// <exception cref="InvalidOperationException">服务容器未初始化</exception>
    public static IServiceProvider Services
    {
        get => field ?? throw new InvalidOperationException($"{nameof(Services)} is not initialized.");
        set;
    }

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        // 获取 Toast 提示视图模型, 以便在未处理异常时显示提示
        // var toastViewModel = Services.GetRequiredService<ToastViewModel>();

        // 处理未处理的 UI 线程异常, 显示 Toast 提示并写入日志文件
        Avalonia.Threading.Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            var ex = e.Exception;
            // UnhandledExceptionHelper.HandleException(false, ex);
            // toastViewModel.Show(
            //     $"发生未处理的 UI 线程异常: {ex.Message}, 阅读 " +
            //     Infrastructure.Constants.UnhandledExceptionLogFilePath +
            //     " 以查看详细信息"
            // );
            e.Handled = true;
        };

        // 处理未处理的任务异常, 显示 Toast 提示并写入日志文件
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            // UnhandledExceptionHelper.HandleException(false, e.Exception);
            // toastViewModel.Show(
            //     $"发生未处理的任务异常: {e.Exception.Message}, 阅读 " +
            //     Infrastructure.Constants.UnhandledExceptionLogFilePath +
            //     " 以查看详细信息"
            // );
            e.SetObserved();
        };

        // 按配置的主题模式应用主题
        // Current?.RequestedThemeVariant = Services.GetRequiredService<UIOptions>().Theme switch
        // {
        //     ThemeMode.Light => ThemeVariant.Light,
        //     ThemeMode.Dark => ThemeVariant.Dark,
        //     _ => ThemeVariant.Default
        // };

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // desktop.MainWindow = new ShellWindow
            // {
            //     DataContext = Services.GetRequiredService<ShellViewModel>()
            // };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            // singleView.MainView = new ShellView
            // {
            //     DataContext = Services.GetRequiredService<ShellViewModel>()
            // };
        }
    }
}
