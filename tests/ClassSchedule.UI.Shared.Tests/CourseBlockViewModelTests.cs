using Avalonia.Media;
using Avalonia.Media.Immutable;
using ClassSchedule.Domain.Models;
using ClassSchedule.UI.Shared.ViewModels;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="CourseBlockViewModel"/> 的单元测试
/// </summary>
public sealed class CourseBlockViewModelTests
{
    /// <summary>
    /// 课程的唯一标识符
    /// </summary>
    private static readonly Guid SampleCourseId = Guid.NewGuid();

    /// <summary>
    /// 课程片段的唯一标识符
    /// </summary>
    private static readonly Guid SampleFragmentId = Guid.NewGuid();

    /// <summary>
    /// 创建一个课程块
    /// </summary>
    /// <param name="weekday">上课日期</param>
    /// <param name="period">上课节次</param>
    /// <param name="color">课程颜色</param>
    /// <returns>课程块</returns>
    private static CourseBlockViewModel CreateBlock(
        Weekday weekday = Weekday.Monday,
        OrdinalRange? period = null,
        string color = "#FF5722")
    {
        return new(
            SampleCourseId,
            SampleFragmentId,
            weekday,
            period ?? new(1, 2),
            [new(1, 16)],
            "高等数学",
            color,
            "教一楼 101",
            "张老师"
        );
    }

    /// <summary>
    /// 验证构造函数设置的字段透传到属性
    /// </summary>
    [Fact]
    public void Ctor_传入字段_透传到属性()
    {
        var period = new OrdinalRange(3, 4);
        IReadOnlyList<OrdinalRange> weeks = [new(1, 8), new(12, 16)];

        var block = new CourseBlockViewModel(
            SampleCourseId,
            SampleFragmentId,
            Weekday.Wednesday,
            period,
            weeks,
            "高等数学",
            "#FF5722",
            "教一楼 101",
            "张老师"
        );

        Assert.Equal(SampleCourseId, block.CourseId);
        Assert.Equal(SampleFragmentId, block.FragmentId);
        Assert.Equal(Weekday.Wednesday, block.Weekday);
        Assert.Equal(period, block.Period);
        Assert.Equal(weeks, block.Weeks);
        Assert.Equal("高等数学", block.Name);
        Assert.Equal("#FF5722", block.Color);
        Assert.Equal("教一楼 101", block.Location);
        Assert.Equal("张老师", block.Teacher);
    }

    /// <summary>
    /// 验证课程块的行号从零起算, 节次一落在第一行
    /// </summary>
    [Fact]
    public void Row_第一节_为零()
    {
        var block = CreateBlock(period: new(1, 2));

        Assert.Equal(0, block.Row);
    }

    /// <summary>
    /// 验证课程块的行号是节次序号减一
    /// </summary>
    [Fact]
    public void Row_第四节_为三()
    {
        var block = CreateBlock(period: new(4, 5));

        Assert.Equal(3, block.Row);
    }

    /// <summary>
    /// 验证课程块的列号从零起算, 周一落在第一列
    /// </summary>
    [Fact]
    public void Column_周一_为零()
    {
        var block = CreateBlock(weekday: Weekday.Monday);

        Assert.Equal(0, block.Column);
    }

    /// <summary>
    /// 验证课程块的列号是星期序号减一, 周日落在第七列
    /// </summary>
    [Fact]
    public void Column_周日_为六()
    {
        var block = CreateBlock(weekday: Weekday.Sunday);

        Assert.Equal(6, block.Column);
    }

    /// <summary>
    /// 验证课程块的列号与星期整数序号对齐, 停用周末之后列定义才不会与块错位
    /// </summary>
    [Fact]
    public void Column_全部星期_为星期序号减一()
    {
        foreach (var weekday in Enum.GetValues<Weekday>())
        {
            var block = CreateBlock(weekday: weekday);

            Assert.Equal((int)weekday - 1, block.Column);
        }
    }

    /// <summary>
    /// 验证只占一个节次的课程块纵向跨一行
    /// </summary>
    [Fact]
    public void RowSpan_单个节次_跨一行()
    {
        var block = CreateBlock(period: new(3, 3));

        Assert.Equal(1, block.RowSpan);
    }

    /// <summary>
    /// 验证跨节次的课程块把首尾都算进去, 两端闭合的区间若按差值算就会少一行
    /// </summary>
    [Fact]
    public void RowSpan_连续节次_首尾都计入()
    {
        var block = CreateBlock(period: new(3, 4));

        Assert.Equal(2, block.RowSpan);
    }

    /// <summary>
    /// 验证课程块的行号加跨行数正好接上区间末尾, 网格上的块才不会被截断
    /// </summary>
    [Fact]
    public void Row与RowSpan_连续节次_覆盖区间全部行()
    {
        var period = new OrdinalRange(3, 4);

        var block = CreateBlock(period: period);

        Assert.Equal(period.Start - 1, block.Row);
        Assert.Equal(period.End - 1, block.Row + block.RowSpan - 1);
    }

    /// <summary>
    /// 验证颜色解析成背景画刷, 并且颜色与原色串一致
    /// </summary>
    [Fact]
    public void Background_合法颜色_解析为对应画刷()
    {
        var block = CreateBlock(color: "#FF5722");

        var brush = Assert.IsType<ImmutableSolidColorBrush>(block.Background);

        Assert.Equal(Color.Parse("#FF5722"), brush.Color);
    }

    /// <summary>
    /// 验证带透明度的颜色也能解析
    /// </summary>
    [Fact]
    public void Background_带透明度颜色_解析为对应画刷()
    {
        var block = CreateBlock(color: "#80FF5722");

        var brush = Assert.IsType<ImmutableSolidColorBrush>(block.Background);

        Assert.Equal(Color.Parse("#80FF5722"), brush.Color);
    }

    /// <summary>
    /// 验证颜色非法时返回空画刷而不是抛异常, 一条脏数据不该让整张网格渲染崩掉
    /// </summary>
    [Fact]
    public void Background_颜色非法_返回空()
    {
        var block = CreateBlock(color: "深红色");

        Assert.Null(block.Background);
    }

    /// <summary>
    /// 验证空颜色串返回空画刷
    /// </summary>
    [Fact]
    public void Background_空颜色_返回空()
    {
        var block = CreateBlock(color: "");

        Assert.Null(block.Background);
    }

    /// <summary>
    /// 验证地点与教师允许为空, 空值原样透传而不是被替换成空串
    /// </summary>
    [Fact]
    public void Ctor_地点与教师为空_透传为空()
    {
        var block = new CourseBlockViewModel(
            SampleCourseId,
            SampleFragmentId,
            Weekday.Monday,
            new(1, 2),
            [new(1, 16)],
            "高等数学",
            "#FF5722",
            null,
            null
        );

        Assert.Null(block.Location);
        Assert.Null(block.Teacher);
    }
}
