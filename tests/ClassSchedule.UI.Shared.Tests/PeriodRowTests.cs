using ClassSchedule.UI.Shared.Models;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="PeriodRow"/> 的单元测试
/// </summary>
public sealed class PeriodRowTests
{
    /// <summary>
    /// 验证构造函数设置的字段透传到属性
    /// </summary>
    [Fact]
    public void Ctor_传入节次与时间_透传到属性()
    {
        var startTime = new TimeOnly(8, 0);
        var endTime = new TimeOnly(8, 45);

        var row = new PeriodRow(3, startTime, endTime);

        Assert.Equal(3, row.Ordinal);
        Assert.Equal(startTime, row.StartTime);
        Assert.Equal(endTime, row.EndTime);
    }

    /// <summary>
    /// 验证序号文本补零到两位, 个位数不补零就会与十位数左右错位
    /// </summary>
    [Fact]
    public void OrdinalText_个位序号_补零到两位()
    {
        var row = new PeriodRow(3, new(8, 0), new(8, 45));

        Assert.Equal("03", row.OrdinalText);
    }

    /// <summary>
    /// 验证节次序号越过个位数之后仍然保持两位
    /// </summary>
    [Fact]
    public void OrdinalText_两位序号_保持两位()
    {
        var row = new PeriodRow(10, new(8, 0), new(8, 45));

        Assert.Equal("10", row.OrdinalText);
    }

    /// <summary>
    /// 验证起止时间按 24 小时制到分钟显示
    /// </summary>
    [Fact]
    public void StartText与EndText_起止时间_按24小时制到分钟()
    {
        var row = new PeriodRow(1, new(13, 5), new(13, 50));

        Assert.Equal("13:05", row.StartText);
        Assert.Equal("13:50", row.EndText);
    }

    /// <summary>
    /// 验证零点过后的时间用 24 小时制而不是 12 小时制
    /// </summary>
    [Fact]
    public void StartText_零点_显示为24小时制()
    {
        var row = new PeriodRow(1, new(0, 0), new(0, 45));

        Assert.Equal("00:00", row.StartText);
    }
}
