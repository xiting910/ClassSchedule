using ClassSchedule.Domain.Models;

namespace ClassSchedule.Domain.Tests;

/// <summary>
/// <see cref="WeekRange"/> 的单元测试类
/// </summary>
public sealed class WeekRangeTests
{
    /// <summary>
    /// 验证构造函数在有效范围内正确设置属性
    /// </summary>
    [Fact]
    public void Constructor_ValidRange_ShouldSetProperties()
    {
        var range = new WeekRange(1, 16);

        Assert.Equal(1, range.StartWeek);
        Assert.Equal(16, range.EndWeek);
    }

    /// <summary>
    /// 验证构造函数在起始周号等于结束周号时仍然有效, 并正确设置属性
    /// </summary>
    [Fact]
    public void Constructor_SingleWeek_ShouldSucceed()
    {
        var range = new WeekRange(5, 5);

        Assert.Equal(5, range.StartWeek);
        Assert.Equal(5, range.EndWeek);
    }

    /// <summary>
    /// 验证构造函数在起始周号为非正整数时抛出 <see cref="ArgumentOutOfRangeException"/>
    /// </summary>
    /// <param name="startWeek">起始周号</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_StartWeekNonPositive_ShouldThrowArgumentOutOfRangeException(int startWeek)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new WeekRange(startWeek, 10));
        Assert.Equal(nameof(startWeek), ex.ParamName);
    }

    /// <summary>
    /// 验证构造函数在结束周号小于起始周号时抛出 <see cref="ArgumentOutOfRangeException"/>
    /// </summary>
    /// <param name="startWeek">起始周号</param>
    /// <param name="endWeek">结束周号</param>
    [Theory]
    [InlineData(5, 4)]
    [InlineData(10, 1)]
    public void Constructor_EndWeekLessThanStartWeek_ShouldThrowArgumentOutOfRangeException(int startWeek, int endWeek)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new WeekRange(startWeek, endWeek));
        Assert.Equal(nameof(endWeek), ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="WeekRange.ContainsWeek"/> 方法在周号在范围内时返回 <see langword="true"/>
    /// </summary>
    /// <param name="startWeek">起始周号</param>
    /// <param name="endWeek">结束周号</param>
    /// <param name="week">要判断的周号</param>
    [Theory]
    [InlineData(1, 16, 1)]
    [InlineData(1, 16, 16)]
    [InlineData(1, 16, 8)]
    [InlineData(5, 5, 5)]
    public void ContainsWeek_WeekInRange_ShouldReturnTrue(int startWeek, int endWeek, int week)
    {
        var range = new WeekRange(startWeek, endWeek);

        Assert.True(range.ContainsWeek(week));
    }

    /// <summary>
    /// 验证 <see cref="WeekRange.ContainsWeek"/> 方法在周号不在范围内时返回 <see langword="false"/>
    /// </summary>
    /// <param name="startWeek">起始周号</param>
    /// <param name="endWeek">结束周号</param>
    /// <param name="week">要判断的周号</param>
    [Theory]
    [InlineData(1, 16, 0)]
    [InlineData(1, 16, 17)]
    [InlineData(5, 5, 4)]
    [InlineData(5, 5, 6)]
    public void ContainsWeek_WeekOutOfRange_ShouldReturnFalse(int startWeek, int endWeek, int week)
    {
        var range = new WeekRange(startWeek, endWeek);

        Assert.False(range.ContainsWeek(week));
    }

    /// <summary>
    /// 验证 <see cref="WeekRange.Intersects"/> 方法在两段周号范围有交集时返回 <see langword="true"/>
    /// </summary>
    /// <param name="start1">第一段起始周号</param>
    /// <param name="end1">第一段结束周号</param>
    /// <param name="start2">第二段起始周号</param>
    /// <param name="end2">第二段结束周号</param>
    [Theory]
    [InlineData(1, 8, 5, 12)]
    [InlineData(1, 16, 1, 16)]
    [InlineData(1, 16, 5, 10)]
    [InlineData(5, 10, 1, 16)]
    [InlineData(1, 5, 5, 10)]
    public void Intersects_RangesOverlap_ShouldReturnTrue(int start1, int end1, int start2, int end2)
    {
        var range1 = new WeekRange(start1, end1);
        var range2 = new WeekRange(start2, end2);

        Assert.True(range1.Intersects(range2));
    }

    /// <summary>
    /// 验证 <see cref="WeekRange.Intersects"/> 方法在两段周号范围没有交集时返回 <see langword="false"/>
    /// </summary>
    /// <param name="start1">第一段起始周号</param>
    /// <param name="end1">第一段结束周号</param>
    /// <param name="start2">第二段起始周号</param>
    /// <param name="end2">第二段结束周号</param>
    [Theory]
    [InlineData(1, 5, 6, 10)]
    [InlineData(1, 3, 10, 16)]
    public void Intersects_RangesDoNotOverlap_ShouldReturnFalse(int start1, int end1, int start2, int end2)
    {
        var range1 = new WeekRange(start1, end1);
        var range2 = new WeekRange(start2, end2);

        Assert.False(range1.Intersects(range2));
    }

    /// <summary>
    /// 验证 <see cref="WeekRange.Intersects"/> 方法在两段周号范围有交集时返回 <see langword="true"/>
    /// </summary>
    /// <param name="start1">第一段起始周号</param>
    /// <param name="end1">第一段结束周号</param>
    /// <param name="start2">第二段起始周号</param>
    /// <param name="end2">第二段结束周号</param>
    /// <param name="expected">期望的返回值</param>
    [Theory]
    [InlineData(1, 8, 5, 12, true)]
    [InlineData(1, 5, 6, 10, false)]
    public void Intersects_ShouldBeCommutative(int start1, int end1, int start2, int end2, bool expected)
    {
        var range1 = new WeekRange(start1, end1);
        var range2 = new WeekRange(start2, end2);

        Assert.Equal(expected, range1.Intersects(range2));
        Assert.Equal(expected, range2.Intersects(range1));
    }

    /// <summary>
    /// 验证 <see cref="WeekRange.Equals"/> 方法在两个周号范围的值相同时返回 <see langword="true"/>
    /// </summary>
    [Fact]
    public void Equals_SameValues_ShouldBeEqual()
    {
        var range1 = new WeekRange(1, 16);
        var range2 = new WeekRange(1, 16);

        Assert.Equal(range1, range2);
    }

    /// <summary>
    /// 验证 <see cref="WeekRange.Equals"/> 方法在两个周号范围的值不同时返回 <see langword="false"/>
    /// </summary>
    [Fact]
    public void Equals_DifferentValues_ShouldNotBeEqual()
    {
        var range1 = new WeekRange(1, 16);
        var range2 = new WeekRange(1, 8);

        Assert.NotEqual(range1, range2);
    }

    /// <summary>
    /// 验证 <see cref="WeekRange.GetHashCode"/> 方法在两个周号范围的值相同时返回相同的哈希码
    /// </summary>
    [Fact]
    public void GetHashCode_SameValues_ShouldBeSame()
    {
        var range1 = new WeekRange(1, 16);
        var range2 = new WeekRange(1, 16);

        Assert.Equal(range1.GetHashCode(), range2.GetHashCode());
    }
}
