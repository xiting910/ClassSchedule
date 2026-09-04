using Avalonia.Controls.ApplicationLifetimes;
using Moq;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="ApplicationLifetimeNotSupportedException"/> 的单元测试
/// </summary>
public sealed class ApplicationLifetimeNotSupportedExceptionTests
{
    /// <summary>
    /// 验证传入 <see langword="null"/> 生命周期时类型名为 "null" 且消息包含之
    /// </summary>
    [Fact]
    public void Ctor_null生命周期_LifetimeTypeName为null且消息含null()
    {
        var exception = new ApplicationLifetimeNotSupportedException(null);

        Assert.Equal("null", exception.LifetimeTypeName);
        Assert.Equal("The application lifetime 'null' is not supported.", exception.Message);
        _ = Assert.IsType<NotSupportedException>(exception, false);
    }

    /// <summary>
    /// 验证传入生命周期对象时类型名取实际类型全名并写入消息
    /// </summary>
    [Fact]
    public void Ctor_带生命周期_类型名取自实际类型全名()
    {
        var lifetime = new Mock<IApplicationLifetime>().Object;
        var typeName = lifetime.GetType().FullName;

        var exception = new ApplicationLifetimeNotSupportedException(lifetime);

        Assert.Equal(typeName, exception.LifetimeTypeName);
        Assert.Contains(typeName!, exception.Message);
        Assert.StartsWith("The application lifetime '", exception.Message);
    }
}
