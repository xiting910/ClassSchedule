using ClassSchedule.UI.Shared.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

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
    /// 确认回调抛出异常时的异常信息
    /// </summary>
    private const string ExceptionMessage = "删除时发生异常";

    /// <summary>
    /// 创建带默认时长的 UI 选项
    /// </summary>
    /// <returns>UI 选项</returns>
    private static UIOptions CreateUIOptions()
    {
        var data = new Dictionary<string, string?>
        {
            [$"{nameof(UIOptions)}:{nameof(UIOptions.ToastDurationSeconds)}"] = "5",
            [$"{nameof(UIOptions)}:{nameof(UIOptions.MaxToastCount)}"] = "5"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(data).Build();
        return new(NullLogger<UIOptions>.Instance, config);
    }

    /// <summary>
    /// 验证构造函数设置的属性
    /// </summary>
    [Fact]
    public async Task Ctor_传入文案_属性透传()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(() =>
        {
            var toast = new ToastViewModel(NullLogger<ToastViewModel>.Instance, CreateUIOptions());
            var viewModel = new ConfirmViewModel(
                Title,
                Message,
                ConfirmText,
                () => Task.CompletedTask,
                () => Task.CompletedTask,
                toast,
                NullLogger<ConfirmViewModel>.Instance
            );

            Assert.Equal(Title, viewModel.Title);
            Assert.Equal(Message, viewModel.Message);
            Assert.Equal(ConfirmText, viewModel.ConfirmText);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证确认时先执行确认回调, 再请求关闭
    /// </summary>
    [Fact]
    public async Task ConfirmAsync_有回调_执行回调并请求关闭()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var calls = new List<string>();
            var toast = new ToastViewModel(NullLogger<ToastViewModel>.Instance, CreateUIOptions());
            var viewModel = new ConfirmViewModel(
                Title,
                Message,
                ConfirmText,
                () => { calls.Add("confirm"); return Task.CompletedTask; },
                () => { calls.Add("closing"); return Task.CompletedTask; },
                toast,
                NullLogger<ConfirmViewModel>.Instance
            );

            await viewModel.ConfirmCommand.ExecuteAsync(null);

            Assert.Equal(["confirm", "closing"], calls);
            Assert.Empty(toast.Items);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证取消时不执行确认回调, 但仍请求关闭
    /// </summary>
    [Fact]
    public async Task CancelAsync_取消_不执行确认回调但仍请求关闭()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var confirmed = false;
            var closed = false;
            var toast = new ToastViewModel(NullLogger<ToastViewModel>.Instance, CreateUIOptions());
            var viewModel = new ConfirmViewModel(
                Title,
                Message,
                ConfirmText,
                () => { confirmed = true; return Task.CompletedTask; },
                () => { closed = true; return Task.CompletedTask; },
                toast,
                NullLogger<ConfirmViewModel>.Instance
            );

            await viewModel.CancelCommand.ExecuteAsync(null);

            Assert.False(confirmed);
            Assert.True(closed);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证确认回调抛出异常时弹出提示, 记录日志并照样关闭
    /// </summary>
    [Fact]
    public async Task ConfirmAsync_回调抛异常_弹出提示并记录日志且照样关闭()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var closed = false;
            var toast = new ToastViewModel(NullLogger<ToastViewModel>.Instance, CreateUIOptions());
            var logger = new Mock<ILogger<ConfirmViewModel>>();
            _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            var viewModel = new ConfirmViewModel(
                Title,
                Message,
                ConfirmText,
                () => Task.FromException(new InvalidOperationException(ExceptionMessage)),
                () => { closed = true; return Task.CompletedTask; },
                toast,
                logger.Object
            );

            await viewModel.ConfirmCommand.ExecuteAsync(null);

            Assert.True(closed);
            var item = Assert.Single(toast.Items);
            Assert.Equal($"操作失败: {ExceptionMessage}", item.Message);
            logger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once
            );

            toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证确认回调执行完成之前不请求关闭, 完成之后立即请求
    /// </summary>
    [Fact]
    public async Task ConfirmAsync_回调执行中_不请求关闭()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var confirmed = false;
            var closed = false;
            var toast = new ToastViewModel(NullLogger<ToastViewModel>.Instance, CreateUIOptions());
            var completion = new TaskCompletionSource();
            var viewModel = new ConfirmViewModel(
                Title,
                Message,
                ConfirmText,
                () => { confirmed = true; return completion.Task; },
                () => { closed = true; return Task.CompletedTask; },
                toast,
                NullLogger<ConfirmViewModel>.Instance
            );

            var executing = viewModel.ConfirmCommand.ExecuteAsync(null);

            Assert.True(confirmed);
            Assert.False(closed);
            Assert.False(viewModel.ConfirmCommand.CanExecute(null));

            completion.SetResult();
            await executing;

            Assert.True(closed);

            return 0;
        }, TestContext.Current.CancellationToken);
    }
}
