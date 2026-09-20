using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 浮层宿主视图模型, 用于显示浮层内容
/// </summary>
public sealed partial class OverlayHostViewModel : ObservableObject
{
    /// <summary>
    /// 当前浮层视图模型
    /// </summary>
    [ObservableProperty]
    public partial ObservableObject? Current { get; set; }

    /// <summary>
    /// 当前是否存在浮层
    /// </summary>
    [ObservableProperty]
    public partial bool HasOverlay { get; set; }

    /// <summary>
    /// 遮罩透明度, 用于驱动遮罩的淡入动画
    /// </summary>
    [ObservableProperty]
    public partial double MaskOpacity { get; set; }

    /// <summary>
    /// 打开确认对话框浮层
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="message">说明文本</param>
    /// <param name="confirmText">确认按钮的文案</param>
    /// <param name="onConfirm">确认回调</param>
    public void OpenConfirmOverlay(string title, string message, string confirmText, Action onConfirm)
    {
        var confirm = new ConfirmViewModel(title, message, confirmText, Close, onConfirm);

        Current = confirm;
        HasOverlay = true;
        MaskOpacity = Constants.MaxRatio;

        // 此时 ConfirmViewModel 还未绑定到视图, 需要在 UI 线程上延迟设置动画属性
        Dispatcher.UIThread.Post(() =>
        {
            confirm.Opacity = Constants.MaxRatio;
            confirm.OffsetY = 0;
        });
    }

    /// <summary>
    /// 关闭当前浮层
    /// </summary>
    public void Close()
    {
        MaskOpacity = 0;
        HasOverlay = false;
        Current = null;
    }
}
