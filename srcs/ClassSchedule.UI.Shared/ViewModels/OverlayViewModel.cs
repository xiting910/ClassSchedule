using CommunityToolkit.Mvvm.ComponentModel;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 浮层的抽象基类视图模型, 用于驱动浮层的入场动画
/// </summary>
/// <param name="opacity">透明度</param>
/// <param name="offsetY">Y 轴偏移量</param>
public abstract partial class OverlayViewModel(double opacity, double offsetY) : ObservableObject
{
    /// <summary>
    /// 透明度
    /// </summary>
    [ObservableProperty]
    public partial double Opacity { get; set; } = opacity;

    /// <summary>
    /// Y 轴偏移量
    /// </summary>
    [ObservableProperty]
    public partial double OffsetY { get; set; } = offsetY;

    /// <summary>
    /// 默认构造函数
    /// </summary>
    protected OverlayViewModel() : this(0.0, 16.0) { }

    /// <summary>
    /// 浮层被打开时的回调, 用于驱动入场动画
    /// </summary>
    public virtual void OnOpen()
    {
        Opacity = Constants.MaxRatio;
        OffsetY = 0;
    }
}
