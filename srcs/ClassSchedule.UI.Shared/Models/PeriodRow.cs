using System;

namespace ClassSchedule.UI.Shared.Models;

/// <summary>
/// 周视图节次列的一行, 承载一个节次的显示内容
/// </summary>
/// <param name="Ordinal">节次序号</param>
/// <param name="StartTime">开始时间</param>
/// <param name="EndTime">结束时间</param>
public sealed record PeriodRow(int Ordinal, TimeOnly StartTime, TimeOnly EndTime)
{
    /// <summary>
    /// 节次序号的显示文本
    /// </summary>
    public string OrdinalText => $"{Ordinal:D2}";

    /// <summary>
    /// 开始时间的显示文本
    /// </summary>
    public string StartText => $"{StartTime:HH:mm}";

    /// <summary>
    /// 结束时间的显示文本
    /// </summary>
    public string EndText => $"{EndTime:HH:mm}";
}
