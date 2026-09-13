namespace ClassSchedule.Domain.Models;

/// <summary>
/// 表示学期中的某一天
/// </summary>
/// <param name="Week">学期周次</param>
/// <param name="Weekday">学期周内的星期几</param>
public readonly record struct SemesterDay(int Week, Weekday Weekday);
