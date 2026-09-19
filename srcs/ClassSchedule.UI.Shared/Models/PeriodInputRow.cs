using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ClassSchedule.UI.Shared.Models;

/// <summary>
/// 新建课表时的单节时间输入行
/// </summary>
/// <param name="ordinal">节次序号</param>
/// <param name="startTime">开始时间, 可以为空表示尚未填写</param>
/// <param name="endTime">结束时间, 可以为空表示尚未填写</param>
public sealed partial class PeriodInputRow(
    int ordinal,
    TimeOnly? startTime,
    TimeOnly? endTime
) : ObservableObject
{
    /// <summary>
    /// 节次序号, 增删行之后由视图模型重排
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrdinalText))]
    public partial int Ordinal { get; set; } = ordinal;

    /// <summary>
    /// 节次序号的显示文本
    /// </summary>
    public string OrdinalText => $"{Ordinal}.";

    /// <summary>
    /// 开始时间
    /// </summary>
    [ObservableProperty]
    public partial TimeOnly? StartTime { get; set; } = startTime;

    /// <summary>
    /// 结束时间
    /// </summary>
    [ObservableProperty]
    public partial TimeOnly? EndTime { get; set; } = endTime;
}
