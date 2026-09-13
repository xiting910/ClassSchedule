using Avalonia;
using ClassSchedule.Infrastructure;
using ClassSchedule.UI.Shared;
using System;

namespace ClassSchedule.UI.Desktop;

/// <summary>
/// 程序入口类
/// </summary>
file static class Program
{
    /// <summary>
    /// 未知异常类
    /// </summary>
    /// <param name="message">异常消息</param>
    private sealed class UnknownException(string? message) : Exception(message);

    /// <summary>
    /// 应用程序入口点
    /// </summary>
    /// <param name="args">命令行参数</param>
    /// <returns>应用程序退出码</returns>
    [STAThread]
    private static int Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception ?? new UnknownException(e.ExceptionObject.ToString());
            UnhandledExceptionHelper.HandleException(e.IsTerminating, ex);
        };

        using var serviceProvider = App.CreateServices<ShellInitializer>();
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
    }
}
