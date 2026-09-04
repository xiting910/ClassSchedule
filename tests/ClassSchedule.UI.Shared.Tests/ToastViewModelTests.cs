using ClassSchedule.UI.Shared.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="ToastViewModel"/> 的单元测试
/// </summary>
public sealed class ToastViewModelTests
{
    /// <summary>
    /// 创建带指定提示时长与最大条数的 UI 选项
    /// </summary>
    /// <param name="durationSeconds">提示时长, 单位秒</param>
    /// <param name="maxCount">最大条数</param>
    /// <returns>UI 选项</returns>
    private static UIOptions CreateUIOptions(double durationSeconds = 5, int maxCount = 5)
    {
        var data = new Dictionary<string, string?>
        {
            [$"{nameof(UIOptions)}:{nameof(UIOptions.ToastDurationSeconds)}"] = durationSeconds.ToString(),
            [$"{nameof(UIOptions)}:{nameof(UIOptions.MaxToastCount)}"] = maxCount.ToString()
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(data).Build();
        return new UIOptions(NullLogger<UIOptions>.Instance, config);
    }

    /// <summary>
    /// 验证显示提示后条目加入集合且更新显隐状态
    /// </summary>
    [Fact]
    public async Task Show_添加条目_并更新HasItems()
    {
        var options = CreateUIOptions();

        await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var viewModel = new ToastViewModel(NullLogger<ToastViewModel>.Instance, options);
            viewModel.Show("hello");

            _ = Assert.Single(viewModel.Items);
            Assert.Equal("hello", viewModel.Items[0].Message);
            Assert.True(viewModel.HasItems);
            Assert.True(viewModel.refreshTimer.IsEnabled);

            viewModel.Items.Clear();
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证提示时长为零时不显示提示
    /// </summary>
    [Fact]
    public async Task Show_时长为零_不显示提示()
    {
        var options = CreateUIOptions(durationSeconds: 0);

        await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var viewModel = new ToastViewModel(NullLogger<ToastViewModel>.Instance, options);
            viewModel.Show("hello");

            Assert.Empty(viewModel.Items);
            Assert.False(viewModel.HasItems);
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证最大条数为零时不显示提示
    /// </summary>
    [Fact]
    public async Task Show_最大条数为零_不显示提示()
    {
        var options = CreateUIOptions(maxCount: 0);

        await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var viewModel = new ToastViewModel(NullLogger<ToastViewModel>.Instance, options);
            viewModel.Show("hello");

            Assert.Empty(viewModel.Items);
            Assert.False(viewModel.HasItems);
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证满员时新提示顶掉最早的一条
    /// </summary>
    [Fact]
    public async Task Show_满员时_顶掉最早条目()
    {
        var options = CreateUIOptions(maxCount: 2);

        await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var viewModel = new ToastViewModel(NullLogger<ToastViewModel>.Instance, options);
            viewModel.Show("first");
            viewModel.Show("second");
            viewModel.Show("third");

            Assert.Equal(2, viewModel.Items.Count);
            Assert.Equal("second", viewModel.Items[0].Message);
            Assert.Equal("third", viewModel.Items[1].Message);

            viewModel.Items.Clear();
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证点击条目时立即关闭该条目并执行其点击回调
    /// </summary>
    [Fact]
    public async Task InvokeClick_移除条目_并执行点击回调()
    {
        var options = CreateUIOptions();
        var invoked = false;

        await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var viewModel = new ToastViewModel(NullLogger<ToastViewModel>.Instance, options);
            viewModel.Show("hello", () => invoked = true);
            var item = viewModel.Items[0];

            viewModel.InvokeClick(item);

            Assert.Empty(viewModel.Items);
            Assert.False(viewModel.HasItems);
            Assert.True(invoked);
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证剩余时间耗尽的条目在刷新回调中被移除并停止计时器
    /// </summary>
    [Fact]
    public async Task Tick_过期条目_被移除并停止计时器()
    {
        // 提示时长 20ms, 等待 100ms 后手动执行刷新回调 (等价于计时器到点)
        var options = CreateUIOptions(durationSeconds: 0.02);

        await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var viewModel = new ToastViewModel(NullLogger<ToastViewModel>.Instance, options);
            viewModel.Show("hello");

            Thread.Sleep(100);
            viewModel.OnRefreshTimerTick(null, EventArgs.Empty);

            Assert.Empty(viewModel.Items);
            Assert.False(viewModel.HasItems);
            Assert.False(viewModel.refreshTimer.IsEnabled);
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证提示条目仍在显示时进度刷新回调不会移除未耗尽条目
    /// </summary>
    [Fact]
    public async Task Tick_未耗尽条目_保留在集合中()
    {
        var options = CreateUIOptions(durationSeconds: 5);

        await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var viewModel = new ToastViewModel(NullLogger<ToastViewModel>.Instance, options);
            viewModel.Show("hello");

            Thread.Sleep(100);
            viewModel.OnRefreshTimerTick(null, EventArgs.Empty);

            _ = Assert.Single(viewModel.Items);
            Assert.True(viewModel.HasItems);

            viewModel.Items.Clear();
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证多条提示按各自的剩余时间先后被移除
    /// </summary>
    [Fact]
    public async Task Tick_多条提示_各自到期后移除()
    {
        var options = CreateUIOptions(durationSeconds: 0.02, maxCount: 3);

        await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var viewModel = new ToastViewModel(NullLogger<ToastViewModel>.Instance, options);
            viewModel.Show("first");
            viewModel.Show("second");

            Thread.Sleep(100);
            viewModel.OnRefreshTimerTick(null, EventArgs.Empty);
            Assert.Empty(viewModel.Items);
            Assert.False(viewModel.HasItems);
        }, TestContext.Current.CancellationToken);
    }
}
