using ClassSchedule.UI.Shared.Models;
using System.ComponentModel;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="Toast"/> 的单元测试
/// </summary>
public sealed class ToastTests
{
    /// <summary>
    /// 测试用的固定显示时长
    /// </summary>
    private static readonly TimeSpan Duration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 验证构造函数设置的初始状态
    /// </summary>
    [Fact]
    public void Ctor_初始状态正确()
    {
        var toast = new Toast("提示", Duration);

        Assert.Equal("提示", toast.Message);
        Assert.Equal(16.0, toast.EnterOffset);
        Assert.Equal(0, toast.EnterOpacity);
        Assert.Equal(Constants.MaxRatio, toast.Progress);
        Assert.False(toast.IsPaused);
    }

    /// <summary>
    /// 验证经过部分时长后按比例扣减进度, 未耗尽时返回 <see langword="false"/>
    /// </summary>
    [Fact]
    public void Tick_经过部分时长_按比例扣减进度()
    {
        var toast = new Toast("提示", Duration);

        Assert.False(toast.Tick(TimeSpan.FromSeconds(1)));
        Assert.Equal(0.8, toast.Progress);
    }

    /// <summary>
    /// 验证多次扣减按累计时间比例计算进度
    /// </summary>
    [Fact]
    public void Tick_多次扣减_进度按累计时间计算()
    {
        var toast = new Toast("提示", Duration);

        _ = toast.Tick(TimeSpan.FromSeconds(1));
        _ = toast.Tick(TimeSpan.FromSeconds(1));

        Assert.Equal(0.6, toast.Progress);
    }

    /// <summary>
    /// 验证时间刚好耗尽时返回 <see langword="true"/> 且进度为 0
    /// </summary>
    [Fact]
    public void Tick_时间刚好耗尽_返回true且进度为0()
    {
        var toast = new Toast("提示", Duration);

        Assert.True(toast.Tick(Duration));
        Assert.Equal(0, toast.Progress);
    }

    /// <summary>
    /// 验证超过总时长时返回 <see langword="true"/> 且进度钳制为 0
    /// </summary>
    [Fact]
    public void Tick_超过总时长_返回true且进度钳制为0()
    {
        var toast = new Toast("提示", Duration);

        Assert.True(toast.Tick(TimeSpan.FromSeconds(10)));
        Assert.Equal(0, toast.Progress);
    }

    /// <summary>
    /// 验证暂停期间扣减被忽略, 进度保持不变
    /// </summary>
    [Fact]
    public void Tick_暂停期间_不扣减进度()
    {
        var toast = new Toast("提示", Duration) { IsPaused = true };

        Assert.False(toast.Tick(TimeSpan.FromSeconds(1)));
        Assert.Equal(Constants.MaxRatio, toast.Progress);
    }

    /// <summary>
    /// 验证暂停解除后继续按剩余时间扣减进度
    /// </summary>
    [Fact]
    public void Tick_暂停后恢复_继续扣减进度()
    {
        var toast = new Toast("提示", Duration)
        {
            IsPaused = true
        };
        _ = toast.Tick(TimeSpan.FromSeconds(1));
        toast.IsPaused = false;

        Assert.False(toast.Tick(TimeSpan.FromSeconds(1)));
        Assert.Equal(0.8, toast.Progress);
    }

    /// <summary>
    /// 验证剩余时间耗尽后继续暂停也不会返回耗尽状态之外的结果 (暂停时剩余时间不再减少)
    /// </summary>
    [Fact]
    public void Tick_耗尽后暂停_保持已耗尽状态()
    {
        var toast = new Toast("提示", Duration);
        _ = toast.Tick(Duration);
        toast.IsPaused = true;

        Assert.True(toast.Tick(TimeSpan.FromSeconds(1)));
    }

    /// <summary>
    /// 验证进度属性变化时触发 <see cref="INotifyPropertyChanged.PropertyChanged"/> 通知
    /// </summary>
    [Fact]
    public void Progress_变化时_触发属性变更通知()
    {
        var toast = new Toast("提示", Duration);
        var changedProperties = new List<string?>();
        toast.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        _ = toast.Tick(TimeSpan.FromSeconds(1));

        Assert.Contains(nameof(Toast.Progress), changedProperties);
        Assert.DoesNotContain(nameof(Toast.Message), changedProperties);
    }

    /// <summary>
    /// 验证设置了点击回调时 <see cref="Toast.InvokeClick"/> 会执行回调
    /// </summary>
    [Fact]
    public void InvokeClick_有回调_执行回调()
    {
        var invoked = false;
        var toast = new Toast("提示", Duration, () => invoked = true);

        toast.InvokeClick();

        Assert.True(invoked);
    }

    /// <summary>
    /// 验证未设置点击回调时 <see cref="Toast.InvokeClick"/> 不抛出异常
    /// </summary>
    [Fact]
    public void InvokeClick_无回调_不抛出异常()
    {
        var toast = new Toast("提示", Duration);

        toast.InvokeClick();
    }
}
