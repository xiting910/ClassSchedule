using ClassSchedule.UI.Shared.Models;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="PeriodInputRow"/> 的单元测试
/// </summary>
public sealed class PeriodInputRowTests
{
    /// <summary>
    /// 验证修改序号时同时通知序号与序号文本, 保证增删行之后界面上的行号跟着重排
    /// </summary>
    [Fact]
    public void Ordinal_修改_同时通知序号与序号文本()
    {
        var row = new PeriodInputRow(1, null, null);
        var changedProperties = new List<string?>();
        row.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        row.Ordinal = 2;

        Assert.Contains(nameof(PeriodInputRow.Ordinal), changedProperties);
        Assert.Contains(nameof(PeriodInputRow.OrdinalText), changedProperties);
        Assert.Equal("2.", row.OrdinalText);
    }

    /// <summary>
    /// 验证序号文本由序号加点号组成
    /// </summary>
    [Fact]
    public void OrdinalText_序号_显示为序号加点号()
    {
        var row = new PeriodInputRow(3, null, null);

        Assert.Equal("3.", row.OrdinalText);
    }

    /// <summary>
    /// 验证构造函数设置的时间透传到开始与结束时间
    /// </summary>
    [Fact]
    public void Ctor_传入时间_透传开始与结束时间()
    {
        var startTime = new TimeOnly(8, 0);
        var endTime = new TimeOnly(8, 45);

        var row = new PeriodInputRow(1, startTime, endTime);

        Assert.Equal(startTime, row.StartTime);
        Assert.Equal(endTime, row.EndTime);
    }
}
