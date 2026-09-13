using ClassSchedule.Domain.Models;
using System;

namespace ClassSchedule.Domain;

/// <summary>
/// <see cref="DayOfWeek"/> 扩展方法类
/// </summary>
public static class DayOfWeekExtensions
{
    /// <summary>
    /// <see cref="DayOfWeek"/> 的扩展块
    /// </summary>
    extension(DayOfWeek dayOfWeek)
    {
        /// <summary>
        /// 将 <see cref="DayOfWeek"/> 转换为 <see cref="Weekday"/> 枚举
        /// </summary>
        /// <returns><see cref="Weekday"/> 枚举</returns>
        /// <exception cref="ArgumentOutOfRangeException">参数不满足约束时抛出</exception>
        public Weekday ToWeekday()
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => Weekday.Monday,
                DayOfWeek.Tuesday => Weekday.Tuesday,
                DayOfWeek.Wednesday => Weekday.Wednesday,
                DayOfWeek.Thursday => Weekday.Thursday,
                DayOfWeek.Friday => Weekday.Friday,
                DayOfWeek.Saturday => Weekday.Saturday,
                DayOfWeek.Sunday => Weekday.Sunday,
                _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null)
            };
        }
    }
}
