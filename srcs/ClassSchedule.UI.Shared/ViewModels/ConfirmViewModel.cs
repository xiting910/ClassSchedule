using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 确认对话框视图模型
/// </summary>
/// <param name="title">标题</param>
/// <param name="message">说明文本</param>
/// <param name="confirmText">确认按钮的文案</param>
/// <param name="onClosing">关闭回调</param>
/// <param name="onConfirm">确认回调</param>
public sealed partial class ConfirmViewModel(
    string title,
    string message,
    string confirmText,
    Action onClosing,
    Action onConfirm
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
    /// 透明度, 用于驱动淡入动画
    /// </summary>
    [ObservableProperty]
    public partial double Opacity { get; set; }

    /// <summary>
    /// Y 轴偏移量, 用于驱动从下方上浮的动画
    /// </summary>
    [ObservableProperty]
    public partial double OffsetY { get; set; } = 16.0;

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
