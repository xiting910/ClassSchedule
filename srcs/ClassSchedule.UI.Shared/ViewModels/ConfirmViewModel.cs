using CommunityToolkit.Mvvm.Input;
using System;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 确认对话框视图模型
/// </summary>
/// <param name="title">标题</param>
/// <param name="message">说明文本</param>
/// <param name="confirmText">确认按钮的文案</param>
/// <param name="onConfirm">确认回调</param>
/// <param name="onClosing">关闭回调</param>
public sealed partial class ConfirmViewModel(
    string title,
    string message,
    string confirmText,
    Action onConfirm,
    Action onClosing
) : OverlayViewModel
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
    /// 关闭并执行确认回调
    /// </summary>
    [RelayCommand]
    private void Confirm()
    {
        onClosing.Invoke();
        onConfirm.Invoke();
    }

    /// <summary>
    /// 不执行任何回调, 直接关闭
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        onClosing.Invoke();
    }
}
