namespace ClassSchedule.Domain.Tests;

/// <summary>
/// <see cref="DateOnlyExtensions"/> 的单元测试类
/// </summary>
public sealed class DateOnlyExtensionsTests
{
    /// <summary>
    /// 测试用的星期一日期
    /// </summary>
    private readonly DateOnly SampleMonday = new(2026, 8, 31);

    /// <summary>
    /// 测试 <see cref="DateOnlyExtensions.GetMonday"/> 方法在日期已经是星期一时返回相同的日期
    /// </summary>
    [Fact]
    public void GetMonday_WhenDateIsMonday_ReturnsSameDate()
    {
        var result = SampleMonday.GetMonday();

        Assert.Equal(SampleMonday, result);
    }

    /// <summary>
    /// 测试 <see cref="DateOnlyExtensions.GetMonday"/> 方法在日期为星期二到星期日时返回正确的星期一日期
    /// </summary>
    [Theory]
    [InlineData(2026, 9, 1)] // Tuesday
    [InlineData(2026, 9, 2)] // Wednesday
    [InlineData(2026, 9, 3)] // Thursday
    [InlineData(2026, 9, 4)] // Friday
    [InlineData(2026, 9, 5)] // Saturday
    [InlineData(2026, 9, 6)] // Sunday
    public void GetMonday_WhenDateIsNotMonday_ReturnsCorrectMonday(int year, int month, int day)
    {
        var date = new DateOnly(year, month, day);

        var result = date.GetMonday();

        Assert.Equal(SampleMonday, result);
    }

    /// <summary>
    /// 测试 <see cref="DateOnlyExtensions.GetMonday"/> 方法在 <see cref="DateOnly.MinValue"/> 时不抛出异常
    /// </summary>
    [Fact]
    public void GetMonday_WhenDateIsMinValue_DoesNotThrow()
    {
        var exception = Record.Exception(() => DateOnly.MinValue.GetMonday());

        Assert.Null(exception);
    }
}
