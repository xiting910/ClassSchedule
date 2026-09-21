using ClassSchedule.UI.Shared.ViewModels;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="ConfirmViewModel"/> 的单元测试
/// </summary>
public sealed class ConfirmViewModelTests
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
    /// 验证构造函数设置的属性
    /// </summary>
    [Fact]
    public async Task Ctor_传入文案_属性透传()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var viewModel = new ConfirmViewModel(Title, Message, ConfirmText, () => { }, () => { });

            Assert.Equal(Title, viewModel.Title);
            Assert.Equal(Message, viewModel.Message);
            Assert.Equal(ConfirmText, viewModel.ConfirmText);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证确认时先关闭浮层再执行确认回调, 顺序反了就成了"边跑边显示对话框"
    /// </summary>
    [Fact]
    public async Task Confirm_有回调_先关闭再执行回调()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var calls = new List<string>();
            var viewModel = new ConfirmViewModel(
                Title,
                Message,
                ConfirmText,
                () => calls.Add("confirm"),
                () => calls.Add("closing")
            );

            viewModel.ConfirmCommand.Execute(null);

            Assert.Equal(["closing", "confirm"], calls);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证取消时不执行确认回调, 但仍执行关闭回调
    /// </summary>
    [Fact]
    public async Task Cancel_取消_不执行确认回调但仍执行关闭回调()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var confirmed = false;
            var closed = false;
            var viewModel = new ConfirmViewModel(
                Title,
                Message,
                ConfirmText,
                () => confirmed = true,
                () => closed = true
            );

            viewModel.CancelCommand.Execute(null);

            Assert.False(confirmed);
            Assert.True(closed);

            return 0;
        }, TestContext.Current.CancellationToken);
    }
}
