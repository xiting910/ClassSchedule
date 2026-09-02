using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ClassSchedule.UI.Shared.Models;

/// <summary>
/// 提示条目类, 用于在右下角显示短暂提示
/// </summary>
/// <param name="message">提示文本</param>
/// <param name="duration">显示时长</param>
/// <param name="clickAction">点击回调</param>
public sealed partial class Toast(
    string message,
    TimeSpan duration,
    Action? clickAction = null
) : ObservableObject
{
    /// <summary>
    /// 提示文本
    /// </summary>
    [ObservableProperty]
    public partial string Message { get; set; } = message;

    /// <summary>
    /// 入场位移偏移
    /// </summary>
    [ObservableProperty]
    public partial double EnterOffset { get; set; } = 16.0;

    /// <summary>
    /// 入场透明度
    /// </summary>
    [ObservableProperty]
    public partial double EnterOpacity { get; set; }

    /// <summary>
    /// 剩余显示时间比例, 驱动底部进度条从满宽缩至零
    /// </summary>
    [ObservableProperty]
    public partial double Progress { get; set; } = Constants.MaxRatio;

    /// <summary>
    /// 是否暂停倒计时
    /// </summary>
    [ObservableProperty]
    public partial bool IsPaused { get; set; }

    /// <summary>
    /// 总显示时长, 用于计算剩余时间比例
    /// </summary>
    private readonly TimeSpan _totalDuration = duration;

    /// <summary>
    /// 点击回调, 点击提示时执行
    /// </summary>
    private readonly Action? _clickAction = clickAction;

    /// <summary>
    /// 剩余显示时间
    /// </summary>
    private TimeSpan _remaining = duration;

    /// <summary>
    /// 按经过时间扣减剩余时间并更新进度条, 返回剩余时间是否耗尽
    /// </summary>
    /// <param name="delta">距上次刷新的时间间隔</param>
    /// <returns><see langword="true"/> 如果剩余时间耗尽, 否则 <see langword="false"/></returns>
    public bool Tick(TimeSpan delta)
    {
        if (!IsPaused)
        {
            _remaining -= delta;
            Progress = Math.Clamp(_remaining / _totalDuration, 0, Constants.MaxRatio);
        }
        return _remaining <= TimeSpan.Zero;
    }

    /// <summary>
    /// 执行点击回调
    /// </summary>
    public void InvokeClick()
    {
        _clickAction?.Invoke();
    }
}
