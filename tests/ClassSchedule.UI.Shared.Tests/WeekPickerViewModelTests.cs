using ClassSchedule.UI.Shared.Models;
using ClassSchedule.UI.Shared.ViewModels;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="WeekPickerViewModel"/> 的单元测试
/// </summary>
public sealed class WeekPickerViewModelTests
{
    /// <summary>
    /// 创建一个周次选择浮层
    /// </summary>
    /// <param name="weeks">周次项列表</param>
    /// <param name="switchWeek">切周回调</param>
    /// <param name="onClosing">关闭浮层回调</param>
    /// <returns>周次选择浮层视图模型</returns>
    private static WeekPickerViewModel CreatePicker(
        IEnumerable<WeekNumberItem>? weeks = null,
        Action<int>? switchWeek = null,
        Action? onClosing = null
    )
    {
        weeks ??= [new(1, true, true), new(2, false, false)];
        switchWeek ??= _ => { };
        onClosing ??= () => { };

        return new(weeks, switchWeek, onClosing);
    }

    /// <summary>
    /// 验证周次项列表透传到属性, 周次与两处标记都不丢
    /// </summary>
    [Fact]
    public async Task Ctor_传入周次项_透传到属性()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var viewModel = CreatePicker([new(7, true, false), new(8, false, true)]);

            Assert.Equal(2, viewModel.Weeks.Count);
            Assert.Equal(7, viewModel.Weeks[0].Week);
            Assert.True(viewModel.Weeks[0].IsCurrent);
            Assert.False(viewModel.Weeks[0].IsDisplayed);
            Assert.Equal(8, viewModel.Weeks[1].Week);
            Assert.False(viewModel.Weeks[1].IsCurrent);
            Assert.True(viewModel.Weeks[1].IsDisplayed);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证构造时就把传入的序列吃进列表, 惰性序列稍后变化不会影响已经列出来的周次
    /// </summary>
    [Fact]
    public async Task Ctor_惰性序列_构造时就固化()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var source = new List<WeekNumberItem> { new(1, false, false) };
            var viewModel = CreatePicker(source.Select(static item => item));
            source.Add(new(2, false, false));

            _ = Assert.Single(viewModel.Weeks);
            Assert.Equal(1, viewModel.Weeks[0].Week);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证继承来的初始态是补间起点, 打开前先停在透明且下移的位置
    /// </summary>
    [Fact]
    public async Task Ctor_新建_停在补间起点()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var viewModel = CreatePicker();

            Assert.Equal(0, viewModel.Opacity);
            Assert.Equal(16.0, viewModel.OffsetY);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证浮层打开时把补间属性推到终点
    /// </summary>
    [Fact]
    public async Task OnOpen_打开_推到补间终点()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var viewModel = CreatePicker();

            viewModel.OnOpen();

            Assert.Equal(Constants.MaxRatio, viewModel.Opacity);
            Assert.Equal(0, viewModel.OffsetY);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证选中时先关浮层再切周, 顺序反了就成了"周已换、浮层还挂在屏幕上"
    /// </summary>
    [Fact]
    public async Task Select_选中某一周_先关浮层再切周()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var host = new OverlayHostViewModel();
            var calls = new List<string>();
            var viewModel = CreatePicker(
                switchWeek: _ => calls.Add("switch"),
                onClosing: () => calls.Add("closing")
            );
            host.Open(viewModel);

            viewModel.Select(new(1, false, false));

            Assert.Equal(["closing", "switch"], calls);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证选中的周次原样交给切周回调
    /// </summary>
    [Fact]
    public async Task Select_选中某一周_把周次交给回调()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var switched = 0;
            var viewModel = CreatePicker([new(7, false, false)], week => switched = week);

            viewModel.Select(new(7, false, false));

            Assert.Equal(7, switched);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证选中一次只关一次浮层也只切一次周, 重复执行会让动画与切周互相打架
    /// </summary>
    [Fact]
    public async Task Select_选中某一周_两个回调各执行一次()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var closeCount = 0;
            var switchCount = 0;
            var viewModel = CreatePicker(
                switchWeek: _ => switchCount++,
                onClosing: () => closeCount++
            );

            viewModel.Select(new(1, false, false));

            Assert.Equal(1, closeCount);
            Assert.Equal(1, switchCount);

            return 0;
        }, TestContext.Current.CancellationToken);
    }
}
