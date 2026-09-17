using Avalonia.Controls;
using ClassSchedule.UI.Shared.ViewModels;
using ClassSchedule.UI.Shared.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="ViewLocator"/> 的单元测试
/// </summary>
public sealed class ViewLocatorTests
{
    /// <summary>
    /// 验证 <see cref="ViewLocator.Match"/> 对 <see langword="null"/> 返回 <see langword="false"/>
    /// </summary>
    [Fact]
    public void Match_null_返回false()
    {
        var locator = new ViewLocator();

        Assert.False(locator.Match(null));
    }

    /// <summary>
    /// 验证 <see cref="ViewLocator.Match"/> 对非视图模型类型返回 <see langword="false"/>
    /// </summary>
    [Fact]
    public void Match_非视图模型类型_返回false()
    {
        var locator = new ViewLocator();

        Assert.False(locator.Match(new object()));
        Assert.False(locator.Match("not a view model"));
    }

    /// <summary>
    /// 验证 <see cref="ViewLocator.Match"/> 对名称以 ViewModel 结尾的类型返回 <see langword="true"/>
    /// </summary>
    [Fact]
    public void Match_视图模型类型_返回true()
    {
        var locator = new ViewLocator();

        Assert.True(locator.Match(new FakePageViewModel()));
    }

    /// <summary>
    /// 验证 <see cref="ViewLocator.Build"/> 对 <see langword="null"/> 返回 <see langword="null"/>
    /// </summary>
    [Fact]
    public async Task Build_null_返回null()
    {
        var locator = new ViewLocator();

        await TestEnvironmentFixture.Session.Dispatch(
            () => Assert.Null(locator.Build(null)),
            TestContext.Current.CancellationToken
        );
    }

    /// <summary>
    /// 验证 <see cref="ViewLocator.Build"/> 能按类型名定位并创建对应视图控件
    /// </summary>
    [Fact]
    public async Task Build_视图模型_创建对应视图控件()
    {
        var locator = new ViewLocator();

        await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var control = locator.Build(new ToastViewModel(
                NullLogger<ToastViewModel>.Instance,
                new(NullLogger<UIOptions>.Instance, new ConfigurationBuilder().Build())
            ));

            _ = Assert.IsType<ToastView>(control);
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证 <see cref="ViewLocator.Build"/> 在找不到对应视图时返回未找到提示控件
    /// </summary>
    [Fact]
    public async Task Build_无对应视图_返回未找到提示()
    {
        var locator = new ViewLocator();

        await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var control = locator.Build(new FakePageViewModel());

            var textBlock = Assert.IsType<TextBlock>(control);
            Assert.Equal($"未找到视图: {typeof(FakePageViewModel).FullName}", textBlock.Text);
        }, TestContext.Current.CancellationToken);
    }
}
