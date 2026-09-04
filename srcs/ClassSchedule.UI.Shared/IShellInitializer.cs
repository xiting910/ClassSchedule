using Avalonia.Controls.ApplicationLifetimes;

namespace ClassSchedule.UI.Shared;

/// <summary>
/// 应用的核心壳视图的初始化器接口, 由平台入口在启动时注入
/// </summary>
public interface IShellInitializer
{
    /// <summary>
    /// 初始化应用的核心壳视图
    /// </summary>
    /// <param name="lifetime">应用程序生命周期</param>
    /// <exception cref="ApplicationLifetimeNotSupportedException">如果应用程序生命周期不受支持</exception>
    void Initialize(IApplicationLifetime? lifetime);
}
