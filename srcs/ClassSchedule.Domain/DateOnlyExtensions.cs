using ClassSchedule.Domain.Models;
using System;

namespace ClassSchedule.Domain;

/// <summary>
/// <see cref="DateOnly"/> 扩展方法类
/// </summary>
public static class DateOnlyExtensions
{
    /// <summary>
    /// <see cref="DateOnly"/> 的扩展块
    /// </summary>
    /// <param name="date">日期</param>
    extension(DateOnly date)
    {
        /// <summary>
        /// 获取当前日期所在周的星期一日期
        /// </summary>
        /// <returns>星期一日期</returns>
        public DateOnly GetMonday()
        {
            const int Monday = (int)Weekday.Monday;
            return date.AddDays(Monday - (int)date.DayOfWeek.ToWeekday());
        }
    }
}
