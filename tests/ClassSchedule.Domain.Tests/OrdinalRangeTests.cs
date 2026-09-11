using ClassSchedule.Domain.Models;

namespace ClassSchedule.Domain.Tests;

/// <summary>
/// <see cref="OrdinalRange"/> 的单元测试类
/// </summary>
public sealed class OrdinalRangeTests
{
    /// <summary>
    /// 验证构造函数在有效范围内正确设置属性
    /// </summary>
    [Fact]
    public void Constructor_ValidRange_ShouldSetProperties()
    {
        var range = new OrdinalRange(1, 16);

        Assert.Equal(1, range.Start);
        Assert.Equal(16, range.End);
    }

    /// <summary>
    /// 验证构造函数在起始序数等于结束序数时仍然有效, 并正确设置属性
    /// </summary>
    [Fact]
    public void Constructor_SingleValue_ShouldSucceed()
    {
        var range = new OrdinalRange(5, 5);

        Assert.Equal(5, range.Start);
        Assert.Equal(5, range.End);
    }

    /// <summary>
    /// 验证构造函数在起始序数为非正整数时抛出 <see cref="ArgumentOutOfRangeException"/>
    /// </summary>
    /// <param name="start">起始序数</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_StartNonPositive_ShouldThrowArgumentOutOfRangeException(int start)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new OrdinalRange(start, 10));
        Assert.Equal("start", ex.ParamName);
    }

    /// <summary>
    /// 验证构造函数在结束序数小于起始序数时抛出 <see cref="ArgumentOutOfRangeException"/>
    /// </summary>
    /// <param name="start">起始序数</param>
    /// <param name="end">结束序数</param>
    [Theory]
    [InlineData(5, 4)]
    [InlineData(10, 1)]
    public void Constructor_EndLessThanStart_ShouldThrowArgumentOutOfRangeException(int start, int end)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new OrdinalRange(start, end));
        Assert.Equal("end", ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.Contains"/> 方法在序数在区间内时返回 <see langword="true"/>
    /// </summary>
    /// <param name="start">起始序数</param>
    /// <param name="end">结束序数</param>
    /// <param name="value">要判断的序数</param>
    [Theory]
    [InlineData(1, 16, 1)]
    [InlineData(1, 16, 16)]
    [InlineData(1, 16, 8)]
    [InlineData(5, 5, 5)]
    public void Contains_ValueInRange_ShouldReturnTrue(int start, int end, int value)
    {
        var range = new OrdinalRange(start, end);

        Assert.True(range.Contains(value));
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.Contains"/> 方法在序数不在区间内时返回 <see langword="false"/>
    /// </summary>
    /// <param name="start">起始序数</param>
    /// <param name="end">结束序数</param>
    /// <param name="value">要判断的序数</param>
    [Theory]
    [InlineData(1, 16, 0)]
    [InlineData(1, 16, 17)]
    [InlineData(5, 5, 4)]
    [InlineData(5, 5, 6)]
    public void Contains_ValueOutOfRange_ShouldReturnFalse(int start, int end, int value)
    {
        var range = new OrdinalRange(start, end);

        Assert.False(range.Contains(value));
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.Intersects"/> 方法在两段区间有交集时返回 <see langword="true"/>
    /// </summary>
    /// <param name="start1">第一段起始序数</param>
    /// <param name="end1">第一段结束序数</param>
    /// <param name="start2">第二段起始序数</param>
    /// <param name="end2">第二段结束序数</param>
    [Theory]
    [InlineData(1, 8, 5, 12)]
    [InlineData(1, 16, 1, 16)]
    [InlineData(1, 16, 5, 10)]
    [InlineData(5, 10, 1, 16)]
    [InlineData(1, 5, 5, 10)]
    public void Intersects_RangesOverlap_ShouldReturnTrue(int start1, int end1, int start2, int end2)
    {
        var range1 = new OrdinalRange(start1, end1);
        var range2 = new OrdinalRange(start2, end2);

        Assert.True(range1.Intersects(range2));
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.Intersects"/> 方法在两段区间没有交集时返回 <see langword="false"/>
    /// </summary>
    /// <param name="start1">第一段起始序数</param>
    /// <param name="end1">第一段结束序数</param>
    /// <param name="start2">第二段起始序数</param>
    /// <param name="end2">第二段结束序数</param>
    [Theory]
    [InlineData(1, 5, 6, 10)]
    [InlineData(1, 3, 10, 16)]
    public void Intersects_RangesDoNotOverlap_ShouldReturnFalse(int start1, int end1, int start2, int end2)
    {
        var range1 = new OrdinalRange(start1, end1);
        var range2 = new OrdinalRange(start2, end2);

        Assert.False(range1.Intersects(range2));
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.Intersects"/> 方法满足交换律
    /// </summary>
    /// <param name="start1">第一段起始序数</param>
    /// <param name="end1">第一段结束序数</param>
    /// <param name="start2">第二段起始序数</param>
    /// <param name="end2">第二段结束序数</param>
    /// <param name="expected">期望的返回值</param>
    [Theory]
    [InlineData(1, 8, 5, 12, true)]
    [InlineData(1, 5, 6, 10, false)]
    public void Intersects_ShouldBeCommutative(int start1, int end1, int start2, int end2, bool expected)
    {
        var range1 = new OrdinalRange(start1, end1);
        var range2 = new OrdinalRange(start2, end2);

        Assert.Equal(expected, range1.Intersects(range2));
        Assert.Equal(expected, range2.Intersects(range1));
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.TryNormalize"/> 方法在列表为 <see langword="null"/> 时抛出
    /// <see cref="ArgumentNullException"/>
    /// </summary>
    [Fact]
    public void TryNormalize_NullRanges_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => OrdinalRange.TryNormalize(null!, out _, out _));
        Assert.Equal("ranges", ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.TryNormalize"/> 方法在空列表时归一化成功, 重叠输出参数为默认值
    /// </summary>
    [Fact]
    public void TryNormalize_EmptyList_ShouldSucceedWithDefaultOutValues()
    {
        List<OrdinalRange> ranges = [];

        var result = OrdinalRange.TryNormalize(ranges, out var range1, out var range2);

        Assert.True(result);
        Assert.Equal(default, range1);
        Assert.Equal(default, range2);
        Assert.Empty(ranges);
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.TryNormalize"/> 方法在只有一个区间时归一化成功且列表不变
    /// </summary>
    [Fact]
    public void TryNormalize_SingleRange_ShouldSucceedUnchanged()
    {
        List<OrdinalRange> ranges = [new(3, 5)];

        var result = OrdinalRange.TryNormalize(ranges, out var range1, out var range2);

        Assert.True(result);
        Assert.Equal(default, range1);
        Assert.Equal(default, range2);
        Assert.Equal([new(3, 5)], ranges);
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.TryNormalize"/> 方法在互不相交且有序的列表上归一化成功且列表不变
    /// </summary>
    [Fact]
    public void TryNormalize_DisjointSortedRanges_ShouldSucceedUnchanged()
    {
        List<OrdinalRange> ranges = [new(1, 3), new(5, 8), new(10, 12)];

        var result = OrdinalRange.TryNormalize(ranges, out _, out _);

        Assert.True(result);
        Assert.Equal([new(1, 3), new(5, 8), new(10, 12)], ranges);
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.TryNormalize"/> 方法将乱序的互不相交列表就地排序
    /// </summary>
    [Fact]
    public void TryNormalize_UnsortedDisjointRanges_ShouldSortInPlace()
    {
        List<OrdinalRange> ranges = [new(5, 8), new(1, 3), new(10, 12)];

        var result = OrdinalRange.TryNormalize(ranges, out _, out _);

        Assert.True(result);
        Assert.Equal([new(1, 3), new(5, 8), new(10, 12)], ranges);
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.TryNormalize"/> 方法将相邻的两个区间合并为一个
    /// </summary>
    [Fact]
    public void TryNormalize_AdjacentRanges_ShouldMergeIntoOne()
    {
        List<OrdinalRange> ranges = [new(1, 8), new(9, 16)];

        var result = OrdinalRange.TryNormalize(ranges, out _, out _);

        Assert.True(result);
        Assert.Equal([new(1, 16)], ranges);
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.TryNormalize"/> 方法将整条相邻链合并为单个区间
    /// </summary>
    [Fact]
    public void TryNormalize_ChainOfAdjacentRanges_ShouldMergeIntoOne()
    {
        List<OrdinalRange> ranges = [new(1, 4), new(5, 8), new(9, 12)];

        var result = OrdinalRange.TryNormalize(ranges, out _, out _);

        Assert.True(result);
        Assert.Equal([new(1, 12)], ranges);
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.TryNormalize"/> 方法在存在重叠区间时失败, 并通过输出参数带出重叠的两个区间
    /// </summary>
    [Fact]
    public void TryNormalize_OverlappingRanges_ShouldFailWithOverlapPair()
    {
        List<OrdinalRange> ranges = [new(1, 5), new(5, 12)];

        var result = OrdinalRange.TryNormalize(ranges, out var range1, out var range2);

        Assert.False(result);
        Assert.Equal(new(1, 5), range1);
        Assert.Equal(new(5, 12), range2);
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.TryNormalize"/> 方法在乱序且重叠的列表上排序后判定重叠并失败
    /// </summary>
    [Fact]
    public void TryNormalize_UnsortedOverlappingRanges_ShouldFail()
    {
        List<OrdinalRange> ranges = [new(5, 12), new(1, 8)];

        var result = OrdinalRange.TryNormalize(ranges, out var range1, out var range2);

        Assert.False(result);
        Assert.Equal(new(1, 8), range1);
        Assert.Equal(new(5, 12), range2);
        Assert.Equal([new(1, 8), new(5, 12)], ranges);
    }

    /// <summary>
    /// 验证 <see cref="OrdinalRange.TryNormalize"/> 方法在右侧相邻段已合并后才遇到左侧重叠时失败,
    /// 列表停留在部分合并的中间态, 输出参数带出当前状态下的重叠对
    /// </summary>
    [Fact]
    public void TryNormalize_OverlapAfterMerge_ShouldFailAndLeavePartiallyMergedList()
    {
        List<OrdinalRange> ranges = [new(1, 8), new(5, 10), new(11, 20), new(21, 30)];

        var result = OrdinalRange.TryNormalize(ranges, out var range1, out var range2);

        Assert.False(result);
        Assert.Equal(new(1, 8), range1);
        Assert.Equal(new(5, 30), range2);
        Assert.Equal([new(1, 8), new(5, 30)], ranges);
    }
}
