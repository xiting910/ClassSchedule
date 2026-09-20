using Avalonia.Threading;
using ClassSchedule.UI.Shared.ViewModels;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="OverlayHostViewModel"/> 的单元测试
/// </summary>
public sealed class OverlayHostViewModelTests
{
    /// <summary>
    /// 对话框标题
    /// </summary>
    private const string Title = "删除片段";

    /// <summary>
    /// 对话框说明文本
    /// </summary>
    private const string Message = "删除后无法恢复, 确定继续?";

    /// <summary>
    /// 确认按钮的文案
    /// </summary>
    private const string ConfirmText = "删除";

    /// <summary>
    /// 验证打开确认对话框时创建视图模型, 并让遮罩进入补间起点
    /// </summary>
    [Fact]
    public async Task OpenConfirmOverlay_无浮层_创建确认对话框并初始化状态()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var host = new OverlayHostViewModel();

            host.OpenConfirmOverlay(Title, Message, ConfirmText, () => { });

            var confirm = Assert.IsType<ConfirmViewModel>(host.Current);
            Assert.Equal(Title, confirm.Title);
            Assert.Equal(Message, confirm.Message);
            Assert.Equal(ConfirmText, confirm.ConfirmText);
            Assert.True(host.HasOverlay);
            Assert.Equal(Constants.MaxRatio, host.MaskOpacity);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证卡片的补间终点在下一帧才写入, 出生即终值会吞掉入场动画, 所以不能同步写
    /// </summary>
    [Fact]
    public async Task OpenConfirmOverlay_延后一帧_把卡片属性置为补间终点()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var host = new OverlayHostViewModel();

            host.OpenConfirmOverlay(Title, Message, ConfirmText, () => { });
            var confirm = Assert.IsType<ConfirmViewModel>(host.Current);

            // 排到 Post 之后, 等它执行完再断言
            await Dispatcher.UIThread.InvokeAsync(() => { });

            Assert.Equal(Constants.MaxRatio, confirm.Opacity);
            Assert.Equal(0, confirm.OffsetY);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证已有浮层时新的请求直接替换掉旧的确认对话框
    /// </summary>
    [Fact]
    public async Task OpenConfirmOverlay_已有浮层_替换旧的确认对话框()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var host = new OverlayHostViewModel();
            host.OpenConfirmOverlay(Title, Message, ConfirmText, () => { });
            var opened = Assert.IsType<ConfirmViewModel>(host.Current);

            host.OpenConfirmOverlay("删除课程", Message, ConfirmText, () => { });

            var replaced = Assert.IsType<ConfirmViewModel>(host.Current);
            Assert.NotSame(opened, replaced);
            Assert.Equal("删除课程", replaced.Title);
            Assert.True(host.HasOverlay);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证关闭浮层时清空内容并复位遮罩透明度
    /// </summary>
    [Fact]
    public async Task Close_有浮层_清空内容并复位遮罩()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var host = new OverlayHostViewModel();
            host.OpenConfirmOverlay(Title, Message, ConfirmText, () => { });

            host.Close();

            Assert.Null(host.Current);
            Assert.False(host.HasOverlay);
            Assert.Equal(0, host.MaskOpacity);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证关闭之后可以重新打开, 不会被上一次的状态挡住
    /// </summary>
    [Fact]
    public async Task OpenConfirmOverlay_关闭之后_可以重新打开()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var host = new OverlayHostViewModel();
            host.OpenConfirmOverlay(Title, Message, ConfirmText, () => { });
            host.Close();

            host.OpenConfirmOverlay("删除课程", Message, ConfirmText, () => { });

            var confirm = Assert.IsType<ConfirmViewModel>(host.Current);
            Assert.Equal("删除课程", confirm.Title);
            Assert.True(host.HasOverlay);
            Assert.Equal(Constants.MaxRatio, host.MaskOpacity);

            return 0;
        }, TestContext.Current.CancellationToken);
    }
}
