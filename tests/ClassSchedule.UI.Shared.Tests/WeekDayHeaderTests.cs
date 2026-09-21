using ClassSchedule.Domain.Models;
using ClassSchedule.UI.Shared.Models;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="WeekDayHeader"/> 的单元测试
/// </summary>
public sealed class WeekDayHeaderTests
{
    /// <summary>
    /// 验证构造函数设置的字段透传到属性
    /// </summary>
    [Fact]
    public void Ctor_传入星期与日期_透传到属性()
    {
        var date = new DateOnly(2026, 8, 31);

        var header = new WeekDayHeader(Weekday.Monday, date, true);

        Assert.Equal(Weekday.Monday, header.Day);
        Assert.Equal(date, header.Date);
        Assert.True(header.IsToday);
    }

    /// <summary>
    /// 验证星期文本取的是枚举上的中文描述, 而不是枚举成员名
    /// </summary>
    [Theory]
    [InlineData(Weekday.Monday, "星期一")]
    [InlineData(Weekday.Thursday, "星期四")]
    [InlineData(Weekday.Sunday, "星期日")]
    public void DayText_传入星期_显示为中文描述(Weekday day, string expected)
    {
        var header = new WeekDayHeader(day, new(2026, 8, 31), false);

        Assert.Equal(expected, header.DayText);
    }

    /// <summary>
    /// 验证日期文本是月日且个位数月份补零, 表头七列才会左边对齐
    /// </summary>
    [Fact]
    public void DateText_个位数月份_补零到两位()
    {
        var header = new WeekDayHeader(Weekday.Monday, new(2026, 9, 7), false);

        Assert.Equal("09-07", header.DateText);
    }

    /// <summary>
    /// 验证日期文本舍弃年份
    /// </summary>
    [Fact]
    public void DateText_跨年日期_不显示年份()
    {
        var header = new WeekDayHeader(Weekday.Monday, new(2027, 1, 3), false);

        Assert.Equal("01-03", header.DateText);
    }

    /// <summary>
    /// 验证今天标记透传到属性, 非今天时为假
    /// </summary>
    [Fact]
    public void IsToday_非今天_为假()
    {
        var header = new WeekDayHeader(Weekday.Monday, new(2026, 8, 31), false);

        Assert.False(header.IsToday);
    }
}
