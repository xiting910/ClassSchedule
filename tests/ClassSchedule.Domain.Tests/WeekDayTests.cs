using ClassSchedule.Domain.Models;

namespace ClassSchedule.Domain.Tests;

/// <summary>
/// <see cref="Weekday"/> 的单元测试类
/// </summary>
public sealed class WeekdayTests
{
    /// <summary>
    /// 验证 <see cref="Weekday"/> 的所有枚举值的 <see cref="EnumExtensions.GetDescription"/>
    /// 方法都应该返回不等于枚举值名称的描述文本, 以确保每个星期都有对应的描述
    /// </summary>
    [Fact]
    public void AllWeekdays_GetDescription_ShouldNotReturnEnumName()
    {
        foreach (var weekday in Enum.GetValues<Weekday>())
        {
            Assert.NotEqual(weekday.ToString(), weekday.GetDescription());
        }
    }
}
