using ClassSchedule.Domain;
using ClassSchedule.Domain.Models;
using System;

namespace ClassSchedule.UI.Shared.Models;

/// <summary>
/// 周视图的表头, 承载一天的表头显示内容
/// </summary>
/// <param name="Day">星期</param>
/// <param name="Date">该天对应的日期</param>
/// <param name="IsToday">是否为今天</param>
public sealed record WeekDayHeader(Weekday Day, DateOnly Date, bool IsToday)
{
    /// <summary>
    /// 表头的星期文本
    /// </summary>
    public string DayText => Day.GetDescription();

    /// <summary>
    /// 表头的日期文本
    /// </summary>
    public string DateText => $"{Date:MM-dd}";
}
