using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using ClassSchedule.Domain;
using ClassSchedule.Infrastructure;
using ClassSchedule.UI.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ClassSchedule.UI.Android;

/// <summary>
/// 主应用程序类
/// </summary>
[Application]
public class MainApplication : AvaloniaAndroidApplication<App>
{
    /// <summary>
    /// 未知异常类
    /// </summary>
    /// <param name="message">异常消息</param>
    private sealed class UnknownException(string? message) : Exception(message);

    /// <summary>
    /// 初始化一个新的 <see cref="MainApplication"/> 实例
    /// </summary>
    /// <param name="javaReference">Java 参考</param>
    /// <param name="transfer">JNI 处理所有权</param>
    protected MainApplication(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer) { }

    /// <summary>
    /// 自定义应用程序构建器
    /// </summary>
    /// <param name="builder">应用程序构建器</param>
    /// <returns>自定义后的应用程序构建器</returns>
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception ?? new UnknownException(e.ExceptionObject.ToString());
            UnhandledExceptionHelper.HandleException(e.IsTerminating, ex);
        };

        var configurationBuilder = new ConfigurationBuilder();
        var config = configurationBuilder.AddJsonFilesFromSettings().Build();

        App.Services = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddLogging(builder => builder.AddFileLogger())
            .AddDomain()
            .AddInfrastructure()
            .AddUIShared()
            .AddSingleton<IShellInitializer, ShellInitializer>()
            .BuildServiceProvider();

        return base.CustomizeAppBuilder(builder).WithInterFont().LogToTrace();
    }
}
