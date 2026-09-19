using System.Globalization;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="TimeOnlyConverter"/> 的单元测试
/// </summary>
public sealed class TimeOnlyConverterTests
{
    /// <summary>
    /// 验证时刻正向转换为时长
    /// </summary>
    [Fact]
    public void Convert_时刻_转换为时长()
    {
        var converter = new TimeOnlyConverter();

        var result = converter.Convert(
            new TimeOnly(8, 0), typeof(TimeSpan), null, CultureInfo.InvariantCulture
        );

        Assert.Equal(new(8, 0, 0), Assert.IsType<TimeSpan>(result));
    }

    /// <summary>
    /// 验证时长反向转换为时刻
    /// </summary>
    [Fact]
    public void ConvertBack_时长_转换为时刻()
    {
        var converter = new TimeOnlyConverter();

        var result = converter.ConvertBack(
            new TimeSpan(14, 30, 0), typeof(TimeOnly), null, CultureInfo.InvariantCulture
        );

        Assert.Equal(new(14, 30), Assert.IsType<TimeOnly>(result));
    }

    /// <summary>
    /// 验证最小时刻往返转换后不变
    /// </summary>
    [Fact]
    public void Convert_最小时刻_往返后不变()
    {
        var converter = new TimeOnlyConverter();

        var span = converter.Convert(TimeOnly.MinValue, typeof(TimeSpan), null, CultureInfo.InvariantCulture);
        var time = converter.ConvertBack(span, typeof(TimeOnly), null, CultureInfo.InvariantCulture);

        Assert.Equal(TimeOnly.MinValue, Assert.IsType<TimeOnly>(time));
    }

    /// <summary>
    /// 验证最大时刻往返转换后不变, 精度不丢失
    /// </summary>
    [Fact]
    public void Convert_最大时刻_往返后不变()
    {
        var converter = new TimeOnlyConverter();

        var span = converter.Convert(TimeOnly.MaxValue, typeof(TimeSpan), null, CultureInfo.InvariantCulture);
        var time = converter.ConvertBack(span, typeof(TimeOnly), null, CultureInfo.InvariantCulture);

        Assert.Equal(TimeOnly.MaxValue, Assert.IsType<TimeOnly>(time));
    }
}
