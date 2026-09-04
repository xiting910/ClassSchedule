using Avalonia.Controls.ApplicationLifetimes;
using System;

namespace ClassSchedule.UI.Shared;

/// <summary>
/// 应用程序生命周期不受支持异常类
/// </summary>
public sealed class ApplicationLifetimeNotSupportedException : NotSupportedException
{
    /// <summary>
    /// 获取应用程序生命周期类型名称
    /// </summary>
    public string LifetimeTypeName { get; }

    /// <summary>
    /// 构造函数, 通过应用程序生命周期初始化异常
    /// </summary>
    /// <param name="lifetime">应用程序生命周期</param>
    public ApplicationLifetimeNotSupportedException(IApplicationLifetime? lifetime)
        : this(lifetime?.GetType().FullName ?? "null") { }

    /// <summary>
    /// 私有构造函数, 通过应用程序生命周期类型名称初始化异常
    /// </summary>
    /// <param name="lifetimeTypeName">应用程序生命周期类型名称</param>
    private ApplicationLifetimeNotSupportedException(string lifetimeTypeName)
        : base($"The application lifetime '{lifetimeTypeName}' is not supported.")
    {
        LifetimeTypeName = lifetimeTypeName;
    }
}
