using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;
using ClassSchedule.Infrastructure.Models;
using ClassSchedule.UI.Shared.Models;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="TimetableListItem"/> 的单元测试
/// </summary>
public sealed class TimetableListItemTests
{
    /// <summary>
    /// 测试用的首周周一日期
    /// </summary>
    private static readonly DateOnly SampleMonday = new(2026, 8, 31);

    /// <summary>
    /// 验证摘要字段透传到行状态
    /// </summary>
    [Fact]
    public void Ctor_摘要字段_透传到行状态()
    {
        var summary = new TimetableSummary(Guid.NewGuid(), "甲课表", SampleMonday, 18);

        var item = new TimetableListItem(summary);

        Assert.Equal(summary.Id, item.Id);
        Assert.Equal("甲课表", item.Name);
        Assert.False(item.IsCurrent);
        Assert.False(item.IsRenaming);
        Assert.Equal("甲课表", item.EditingName);
    }

    /// <summary>
    /// 验证十八周的学期信息, 结束日为第十八周的周日, 且跨年时按实际年份显示
    /// </summary>
    [Fact]
    public void RangeText_十八周_结束日为第十八周周日并跨年()
    {
        var item = new TimetableListItem(new(Guid.NewGuid(), "甲课表", SampleMonday, 18));

        Assert.Equal("2026-08-31 ~ 2027-01-03 · 共 18 周", item.RangeText);
    }

    /// <summary>
    /// 验证单周学期只有一个星期, 结束日为第一周的周日
    /// </summary>
    [Fact]
    public void RangeText_单周_结束日为第一周周日()
    {
        var item = new TimetableListItem(new(Guid.NewGuid(), "甲课表", SampleMonday, 1));

        Assert.Equal("2026-08-31 ~ 2026-09-06 · 共 1 周", item.RangeText);
    }

    /// <summary>
    /// 验证总周数取上限时结束日仍按周数推算
    /// </summary>
    [Fact]
    public void RangeText_三十周_结束日为第三十周周日()
    {
        var item = new TimetableListItem(new(Guid.NewGuid(), "甲课表", SampleMonday, Timetable.MaxTotalWeeks));

        Assert.Equal("2026-08-31 ~ 2027-03-28 · 共 30 周", item.RangeText);
    }

    /// <summary>
    /// 验证学期信息的结束日与域层的学期日换算对齐: 结束日在学期内, 再后一天不在
    /// </summary>
    [Fact]
    public void RangeText_结束日_与域层学期日换算对齐()
    {
        const int TotalWeeks = 18;
        var summary = new TimetableSummary(Guid.NewGuid(), "甲课表", SampleMonday, TotalWeeks);
        var item = new TimetableListItem(summary);
        var timetable = Assert.IsType<SuccessResult<Timetable>>(
            Timetable.Create("甲课表", SampleMonday, TotalWeeks)
        ).Value;
        var lastDay = SampleMonday.AddDays((TotalWeeks * 7) - 1);

        Assert.Contains($"{lastDay:yyyy-MM-dd}", item.RangeText);
        Assert.True(timetable.TryGetSemesterDay(lastDay, out _));
        Assert.False(timetable.TryGetSemesterDay(lastDay.AddDays(1), out _));
    }
}
