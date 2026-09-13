using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;

namespace ClassSchedule.Domain.Tests;

/// <summary>
/// <see cref="Timetable"/> 核心成员的单元测试类
/// </summary>
public sealed class TimetableTests
{
    /// <summary>
    /// 测试用的首周周一日期
    /// </summary>
    private static readonly DateOnly SampleMonday = new(2026, 8, 31);

    /// <summary>
    /// 创建测试用的课程表
    /// </summary>
    /// <param name="totalWeeks">总周数</param>
    /// <returns>课程表</returns>
    private static Timetable CreateTimetable(int totalWeeks = 16)
    {
        var result = Timetable.Create("测试课表", SampleMonday, totalWeeks);
        return Assert.IsType<SuccessResult<Timetable>>(result).Value;
    }

    /// <summary>
    /// 创建指定首周日期与总周数的测试用课程表
    /// </summary>
    /// <param name="firstDay">课程表第一周的任意一天日期</param>
    /// <param name="totalWeeks">总周数</param>
    /// <returns>课程表</returns>
    private static Timetable CreateTimetable(DateOnly firstDay, int totalWeeks)
    {
        var result = Timetable.Create("测试课表", firstDay, totalWeeks);
        return Assert.IsType<SuccessResult<Timetable>>(result).Value;
    }

    /// <summary>
    /// 创建一节测试用的课并返回其课程标识
    /// </summary>
    /// <param name="timetable">课程表</param>
    /// <returns>课程标识</returns>
    private static Guid AddSampleCourse(Timetable timetable)
    {
        var result = timetable.AddCourse("高等数学", "#FF0000");
        return Assert.IsType<SuccessResult<Course>>(result).Value.Id;
    }

    /// <summary>
    /// 验证 <see cref="Timetable.Name"/> 在传入空字符串或空白字符串时抛出 <see cref="ArgumentException"/>
    /// </summary>
    /// <param name="invalidName">非法的课程表名称</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_BlankValue_ShouldThrowArgumentException(string invalidName)
    {
        var timetable = CreateTimetable();

        var ex = Assert.Throws<ArgumentException>(() => timetable.Name = invalidName);
        Assert.Equal(nameof(Timetable.Name), ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.Name"/> 在传入 <see langword="null"/> 时抛出
    /// <see cref="ArgumentNullException"/>
    /// </summary>
    [Fact]
    public void Name_NullValue_ShouldThrowArgumentNullException()
    {
        var timetable = CreateTimetable();

        var ex = Assert.Throws<ArgumentNullException>(() => timetable.Name = null!);
        Assert.Equal(nameof(Timetable.Name), ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.Name"/> 在传入合法名称时正确赋值
    /// </summary>
    [Fact]
    public void Name_ValidValue_ShouldSetProperty()
    {
        var timetable = CreateTimetable();

        timetable.Name = "2026 秋季学期";

        Assert.Equal("2026 秋季学期", timetable.Name);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.FirstMonday"/> 在传入非星期一的日期时自动归一化为该周的星期一
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="day">日</param>
    [Theory]
    [InlineData(2026, 9, 1)] // Tuesday
    [InlineData(2026, 9, 2)] // Wednesday
    [InlineData(2026, 9, 3)] // Thursday
    [InlineData(2026, 9, 4)] // Friday
    [InlineData(2026, 9, 5)] // Saturday
    [InlineData(2026, 9, 6)] // Sunday
    public void FirstMonday_NonMondayDate_ShouldNormalizeToMonday(int year, int month, int day)
    {
        var result = Timetable.Create("测试课表", new DateOnly(year, month, day), 16);

        var timetable = Assert.IsType<SuccessResult<Timetable>>(result).Value;
        Assert.Equal(SampleMonday, timetable.FirstMonday);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.FirstMonday"/> 在传入星期一时保持原值
    /// </summary>
    [Fact]
    public void FirstMonday_Monday_ShouldKeepOriginalValue()
    {
        var result = Timetable.Create("测试课表", SampleMonday, 16);

        var timetable = Assert.IsType<SuccessResult<Timetable>>(result).Value;
        Assert.Equal(SampleMonday, timetable.FirstMonday);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.Create"/> 在总周数为非正整数时抛出 <see cref="ArgumentOutOfRangeException"/>
    /// </summary>
    /// <param name="totalWeeks">非正的总周数</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_TotalWeeksNonPositive_ShouldThrowArgumentOutOfRangeException(int totalWeeks)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => Timetable.Create("测试课表", SampleMonday, totalWeeks)
        );

        Assert.Equal("totalWeeks", ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.Create"/> 在总周数超过 <see cref="Timetable.MaxTotalWeeks"/> 时返回
    /// <see cref="ErrorCode.TotalWeeksExceedLimit"/>
    /// </summary>
    [Fact]
    public void Create_TotalWeeksExceedsMaximum_ShouldReturnFailure()
    {
        var result = Timetable.Create("测试课表", SampleMonday, Timetable.MaxTotalWeeks + 1);

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.TotalWeeksExceedLimit, failure.Code);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.Create"/> 在总周数取上下边界值时均成功
    /// </summary>
    /// <param name="totalWeeks">边界总周数</param>
    [Theory]
    [InlineData(1)]
    [InlineData(Timetable.MaxTotalWeeks)]
    public void Create_TotalWeeksAtBoundary_ShouldSucceed(int totalWeeks)
    {
        var result = Timetable.Create("测试课表", SampleMonday, totalWeeks);

        var timetable = Assert.IsType<SuccessResult<Timetable>>(result).Value;
        Assert.Equal(totalWeeks, timetable.TotalWeeks);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeTotalWeeks"/> 在总周数超过 <see cref="Timetable.MaxTotalWeeks"/> 时返回
    /// <see cref="ErrorCode.TotalWeeksExceedLimit"/>
    /// </summary>
    [Fact]
    public void ChangeTotalWeeks_ExceedsMaximum_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();

        var result = timetable.ChangeTotalWeeks(Timetable.MaxTotalWeeks + 1);

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.TotalWeeksExceedLimit, failure.Code);
        Assert.Equal(16, timetable.TotalWeeks);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeTotalWeeks"/> 在缩小总周数时, 若有课程片段超出新的总周数则返回
    /// <see cref="ErrorCode.WeekOccupied"/>, 移除该片段后再次缩小则成功
    /// </summary>
    [Fact]
    public void ChangeTotalWeeks_ShrinkBelowReferencedWeek_ShouldFailUntilFragmentRemoved()
    {
        var timetable = CreateTimetable(16);
        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions(
            (new(8, 0), new(8, 45)),
            (new(9, 0), new(9, 45))
        ));
        var courseId = AddSampleCourse(timetable);
        var addResult = timetable.AddFragmentToCourse(courseId, Weekday.Monday, new(1, 2), [new(13, 16)]);
        var fragmentId = Assert.IsType<SuccessResult<Fragment>>(addResult).Value.Id;

        var blockedResult = timetable.ChangeTotalWeeks(12);

        var failure = Assert.IsType<FailureResult>(blockedResult);
        Assert.Equal(ErrorCode.WeekOccupied, failure.Code);
        Assert.Equal(16, timetable.TotalWeeks);

        _ = Assert.IsType<SuccessResult>(timetable.RemoveFragmentFromCourse(courseId, fragmentId));

        var retryResult = timetable.ChangeTotalWeeks(12);

        _ = Assert.IsType<SuccessResult>(retryResult);
        Assert.Equal(12, timetable.TotalWeeks);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeTotalWeeks"/> 在放大总周数时成功
    /// </summary>
    [Fact]
    public void ChangeTotalWeeks_Enlarge_ShouldSucceed()
    {
        var timetable = CreateTimetable(16);

        var result = timetable.ChangeTotalWeeks(20);

        _ = Assert.IsType<SuccessResult>(result);
        Assert.Equal(20, timetable.TotalWeeks);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.TryGetSemesterDay"/> 在日期为首周周一时返回第 1 周星期一
    /// </summary>
    [Fact]
    public void TryGetSemesterDay_DateAtFirstMonday_ShouldReturnFirstWeekMonday()
    {
        var timetable = CreateTimetable(new(2026, 3, 2), 16);

        var isWithinSemester = timetable.TryGetSemesterDay(new(2026, 3, 2), out var semesterDay);

        Assert.True(isWithinSemester);
        Assert.Equal(1, semesterDay.Week);
        Assert.Equal(Weekday.Monday, semesterDay.Weekday);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.TryGetSemesterDay"/> 在日期为首周周日时仍返回第 1 周, 即首周是完整 7 天
    /// </summary>
    [Fact]
    public void TryGetSemesterDay_LastDayOfFirstWeek_ShouldReturnFirstWeekSunday()
    {
        var timetable = CreateTimetable(new(2026, 3, 2), 16);

        var isWithinSemester = timetable.TryGetSemesterDay(new(2026, 3, 8), out var semesterDay);

        Assert.True(isWithinSemester);
        Assert.Equal(1, semesterDay.Week);
        Assert.Equal(Weekday.Sunday, semesterDay.Weekday);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.TryGetSemesterDay"/> 在日期为首周之后满 7 天时返回第 2 周
    /// </summary>
    [Fact]
    public void TryGetSemesterDay_DateInSecondWeek_ShouldReturnSecondWeek()
    {
        var timetable = CreateTimetable(new(2026, 3, 2), 16);

        var isWithinSemester = timetable.TryGetSemesterDay(new(2026, 3, 9), out var semesterDay);

        Assert.True(isWithinSemester);
        Assert.Equal(2, semesterDay.Week);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.TryGetSemesterDay"/> 在日期为学期最后一周周日时返回总周数(上边界)
    /// </summary>
    [Fact]
    public void TryGetSemesterDay_LastDayOfSemester_ShouldReturnTotalWeeks()
    {
        var timetable = CreateTimetable(new(2026, 3, 2), 16);

        var isWithinSemester = timetable.TryGetSemesterDay(new(2026, 6, 21), out var semesterDay);

        Assert.True(isWithinSemester);
        Assert.Equal(16, semesterDay.Week);
        Assert.Equal(Weekday.Sunday, semesterDay.Weekday);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.TryGetSemesterDay"/> 在日期为首周周一的前一天时返回
    /// <see langword="false"/> —— 整数除法向零截断而非向下取整, 这是该边界的回归测试
    /// </summary>
    [Fact]
    public void TryGetSemesterDay_DayBeforeFirstMonday_ShouldReturnFailure()
    {
        var timetable = CreateTimetable(new(2026, 3, 2), 16);

        var isWithinSemester = timetable.TryGetSemesterDay(new(2026, 3, 1), out _);

        Assert.False(isWithinSemester);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.TryGetSemesterDay"/> 在日期为学期最后一周周日的次日时返回
    /// <see langword="false"/>
    /// </summary>
    [Fact]
    public void TryGetSemesterDay_DayAfterLastSemesterDay_ShouldReturnFailure()
    {
        var timetable = CreateTimetable(new(2026, 3, 2), 16);

        var isWithinSemester = timetable.TryGetSemesterDay(new(2026, 6, 22), out _);

        Assert.False(isWithinSemester);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.TryGetSemesterDay"/> 在日期远离学期范围(含
    /// <see cref="DateOnly.MinValue"/> 与 <see cref="DateOnly.MaxValue"/>)时返回
    /// <see langword="false"/> 且不抛异常
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="day">日</param>
    [Theory]
    [InlineData(2026, 1, 1)]
    [InlineData(2027, 12, 31)]
    [InlineData(1, 1, 1)]
    [InlineData(9999, 12, 31)]
    public void TryGetSemesterDay_DateFarOutsideSemester_ShouldReturnFailure(int year, int month, int day)
    {
        var timetable = CreateTimetable(new(2026, 3, 2), 16);

        var isWithinSemester = timetable.TryGetSemesterDay(new(year, month, day), out _);

        Assert.False(isWithinSemester);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.TryGetSemesterDay"/> 在学期跨越日历年时周号仍按日期差连续递增
    /// </summary>
    [Fact]
    public void TryGetSemesterDay_SemesterAcrossYearBoundary_ShouldKeepWeekNumberContinuous()
    {
        var timetable = CreateTimetable(SampleMonday, 20);

        var isLastDayOfYearInSemester = timetable.TryGetSemesterDay(new(2026, 12, 31), out var lastDayOfYear);
        var isFirstDayOfYearInSemester = timetable.TryGetSemesterDay(new(2027, 1, 1), out var firstDayOfYear);

        Assert.True(isLastDayOfYearInSemester);
        Assert.True(isFirstDayOfYearInSemester);
        Assert.Equal(18, lastDayOfYear.Week);
        Assert.Equal(18, firstDayOfYear.Week);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.TryGetSemesterDay"/> 以归一化后的首周周一为锚点, 而非创建时传入的那一天
    /// </summary>
    [Fact]
    public void TryGetSemesterDay_NonMondayFirstDay_ShouldUseNormalizedFirstMonday()
    {
        var timetable = CreateTimetable(new(2026, 9, 2), 16);

        var isWithinSemester = timetable.TryGetSemesterDay(SampleMonday, out var semesterDay);

        Assert.True(isWithinSemester);
        Assert.Equal(1, semesterDay.Week);
        Assert.Equal(Weekday.Monday, semesterDay.Weekday);
    }
}
