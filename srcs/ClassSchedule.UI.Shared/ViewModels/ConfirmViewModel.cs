using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 确认对话框视图模型, 由壳在需要二次确认时临时创建
/// </summary>
/// <param name="title">标题</param>
/// <param name="message">说明文本</param>
/// <param name="confirmText">确认按钮的文案</param>
/// <param name="onConfirm">点击确认时执行的回调</param>
/// <param name="onClosing">对话框关闭时的回调</param>
/// <param name="toast">全局提示视图模型</param>
/// <param name="logger">日志记录器</param>
public sealed partial class ConfirmViewModel(
    string title,
    string message,
    string confirmText,
    Func<Task> onConfirm,
    Func<Task> onClosing,
    ToastViewModel toast,
    ILogger<ConfirmViewModel> logger
) : ObservableObject
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; } = title;

    /// <summary>
    /// 说明文本
    /// </summary>
    public string Message { get; } = message;

    /// <summary>
    /// 确认按钮的文案
    /// </summary>
    public string ConfirmText { get; } = confirmText;

    /// <summary>
    /// 执行确认回调并请求关闭
    /// </summary>
    [RelayCommand]
    private async Task ConfirmAsync()
    {
        try
        {
            await onConfirm();
        }
        catch (Exception ex)
        {
            toast.Show($"操作失败: {ex.Message}");
            LogConfirmCallbackException(ex);
        }
        finally
        {
            await onClosing();
        }
    }

    /// <summary>
    /// 不执行任何回调, 直接请求关闭
    /// </summary>
    [RelayCommand]
    private async Task CancelAsync()
    {
        await onClosing();
    }

    /// <summary>
    /// 记录确认对话框回调执行失败的日志
    /// </summary>
    /// <param name="ex">异常</param>
    [LoggerMessage(
        EventId = 0,
        EventName = "ConfirmCallbackException",
        Level = LogLevel.Warning,
        Message = "Confirm dialog callback execution failed"
    )]
    private partial void LogConfirmCallbackException(Exception ex);
}
