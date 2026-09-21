using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

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
    public partial OverlayViewModel? Current { get; set; }

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
    /// 打开浮层
    /// </summary>
    /// <typeparam name="TOverlayViewModel">浮层视图模型类型</typeparam>
    /// <param name="overlay">浮层视图模型</param>
    public void Open<TOverlayViewModel>(TOverlayViewModel overlay) where TOverlayViewModel : OverlayViewModel
    {
        Current = overlay;
        HasOverlay = true;
        MaskOpacity = Constants.MaxRatio;

        // 浮层的入场动画需要在下一帧才执行, 否则会被同步写入的终值吞掉
        Dispatcher.UIThread.Post(overlay.OnOpen);
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
