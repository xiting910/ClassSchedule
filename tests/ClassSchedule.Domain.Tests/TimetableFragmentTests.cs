using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;

namespace ClassSchedule.Domain.Tests;

/// <summary>
/// <see cref="Timetable"/> 课程片段相关成员的单元测试类
/// </summary>
public sealed class TimetableFragmentTests
{
    /// <summary>
    /// 测试用的首周周一日期
    /// </summary>
    private static readonly DateOnly SampleMonday = new(2026, 8, 31);

    /// <summary>
    /// 创建测试用的课程表, 并从 08:00 开始依次添加每节 45 分钟的课程节次定义
    /// </summary>
    /// <param name="totalWeeks">总周数</param>
    /// <param name="periodCount">课程节次定义的数量</param>
    /// <returns>课程表</returns>
    private static Timetable CreateTimetable(int totalWeeks = 16, int periodCount = 12)
    {
        var result = Timetable.Create("测试课表", SampleMonday, totalWeeks);
        var timetable = Assert.IsType<SuccessResult<Timetable>>(result).Value;

        List<(TimeOnly startTime, TimeOnly endTime)> periods = [];
        for (var i = 0; i < periodCount; i++)
        {
            periods.Add((new(8 + i, 0), new(8 + i, 45)));
        }
        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions([.. periods]));

        return timetable;
    }

    /// <summary>
    /// 创建一门测试用的课程并返回其课程标识
    /// </summary>
    /// <param name="timetable">课程表</param>
    /// <param name="name">课程名称</param>
    /// <param name="color">课程颜色</param>
    /// <returns>课程标识</returns>
    private static Guid AddCourse(Timetable timetable, string name = "高等数学", string color = "#FF0000")
    {
        var result = timetable.AddCourse(name, color);
        return Assert.IsType<SuccessResult<Course>>(result).Value.Id;
    }

    /// <summary>
    /// 添加一个测试用的课程片段并返回该课程片段
    /// </summary>
    /// <param name="timetable">课程表</param>
    /// <param name="courseId">课程片段所在课程的唯一标识符</param>
    /// <param name="weekday">上课日期</param>
    /// <param name="period">上课节次</param>
    /// <param name="weeks">上课周次列表</param>
    /// <returns>新创建的课程片段</returns>
    private static Fragment AddSampleFragment(
        Timetable timetable,
        Guid courseId,
        Weekday weekday,
        OrdinalRange period,
        IEnumerable<OrdinalRange> weeks)
    {
        var result = timetable.AddFragmentToCourse(courseId, weekday, period, weeks);
        return Assert.IsType<SuccessResult<Fragment>>(result).Value;
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在上课节次为单个节号或结束节号恰好等于
    /// 课程节次定义数量时均成功
    /// </summary>
    /// <param name="periodStart">上课节次的起始节号</param>
    /// <param name="periodEnd">上课节次的结束节号</param>
    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 12)]
    public void AddFragmentToCourse_PeriodAtBoundary_ShouldSucceed(int periodStart, int periodEnd)
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);

        var fragment = AddSampleFragment(
            timetable, courseId, Weekday.Tuesday, new(periodStart, periodEnd), [new(1, 16)]
        );

        Assert.Equal(new(periodStart, periodEnd), fragment.Period);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在上课节次的结束节号超出课程节次定义数量时返回
    /// <see cref="ErrorCode.PeriodNotFound"/>, 且不添加课程片段
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_PeriodEndExceedsPeriodCount_ShouldReturnFailure()
    {
        var timetable = CreateTimetable(periodCount: 12);
        var courseId = AddCourse(timetable);
        var course = Assert.Single(timetable.Courses);

        var result = timetable.AddFragmentToCourse(courseId, Weekday.Tuesday, new(1, 13), [new(1, 16)]);

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.PeriodNotFound, failure.Code);
        Assert.Empty(course.Fragments);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在上课周次列表为空时抛出
    /// <see cref="ArgumentOutOfRangeException"/>
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_EmptyWeeks_ShouldThrowArgumentOutOfRangeException()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => timetable.AddFragmentToCourse(courseId, Weekday.Tuesday, new(1, 2), [])
        );

        Assert.Equal("weeks", ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在上课周次列表为 <see langword="null"/> 时抛出
    /// <see cref="ArgumentNullException"/>
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_NullWeeks_ShouldThrowArgumentNullException()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);

        var ex = Assert.Throws<ArgumentNullException>(
            () => timetable.AddFragmentToCourse(courseId, Weekday.Tuesday, new(1, 2), null!)
        );

        Assert.Equal("weeks", ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在相邻的上课周次被合并为一个周段
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_AdjacentWeeks_ShouldMergeIntoOneRange()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);

        var fragment = AddSampleFragment(
            timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 4), new(5, 8)]
        );

        Assert.Equal(new(1, 8), Assert.Single(fragment.Weeks));
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在乱序的上课周次被按起始周号升序排列
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_UnsortedWeeks_ShouldSortAscending()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);

        var fragment = AddSampleFragment(
            timetable, courseId, Weekday.Tuesday, new(1, 2), [new(10, 16), new(1, 8)]
        );

        Assert.Equal(2, fragment.Weeks.Count);
        Assert.Equal(new(1, 8), fragment.Weeks[0]);
        Assert.Equal(new(10, 16), fragment.Weeks[1]);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在上课周次存在重叠时返回
    /// <see cref="ErrorCode.WeekOverlap"/>, 且错误信息包含重叠的两个周段
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_OverlappingWeeks_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        var course = Assert.Single(timetable.Courses);

        var result = timetable.AddFragmentToCourse(
            courseId, Weekday.Tuesday, new(1, 2), [new(1, 8), new(5, 12)]
        );

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.WeekOverlap, failure.Code);
        Assert.Contains("1 - 8", failure.Message);
        Assert.Contains("5 - 12", failure.Message);
        Assert.Empty(course.Fragments);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在上课周次超出总周数时返回
    /// <see cref="ErrorCode.WeekNotFound"/>
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_WeeksExceedTotalWeeks_ShouldReturnFailure()
    {
        var timetable = CreateTimetable(16);
        var courseId = AddCourse(timetable);
        var course = Assert.Single(timetable.Courses);

        var result = timetable.AddFragmentToCourse(courseId, Weekday.Tuesday, new(1, 2), [new(1, 20)]);

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.WeekNotFound, failure.Code);
        Assert.Empty(course.Fragments);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在上课周次的结束周号恰好等于总周数时成功
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_WeeksAtTotalWeeksBoundary_ShouldSucceed()
    {
        var timetable = CreateTimetable(16);
        var courseId = AddCourse(timetable);

        var fragment = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        Assert.Equal(new(1, 16), Assert.Single(fragment.Weeks));
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在同一课程的不同课程片段的节次重叠时返回
    /// <see cref="ErrorCode.FragmentOverlap"/>
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_PeriodOverlapsSameCourse_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        var course = Assert.Single(timetable.Courses);
        _ = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        var result = timetable.AddFragmentToCourse(courseId, Weekday.Tuesday, new(2, 3), [new(1, 16)]);

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.FragmentOverlap, failure.Code);
        _ = Assert.Single(course.Fragments);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在同一课程的同一天同一节次但周次不相交时成功
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_SamePeriodOnDisjointWeeks_ShouldSucceed()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        _ = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 8)]);

        var second = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(9, 16)]);

        Assert.Equal(Weekday.Tuesday, second.Weekday);
        Assert.Equal(new(1, 2), second.Period);
        Assert.Equal(new(9, 16), Assert.Single(second.Weeks));
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在同一课程的同一天同一节次但上课日期不同时成功
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_SamePeriodOnDifferentWeekday_ShouldSucceed()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        var course = Assert.Single(timetable.Courses);
        _ = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        var second = AddSampleFragment(timetable, courseId, Weekday.Thursday, new(1, 2), [new(1, 16)]);

        Assert.Equal(Weekday.Thursday, second.Weekday);
        Assert.Equal(2, course.Fragments.Count);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在同一课程的同一天节次不相交且周次相同时成功
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_DisjointPeriodsOnSameWeekday_ShouldSucceed()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        var course = Assert.Single(timetable.Courses);
        _ = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        var second = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(5, 6), [new(1, 16)]);

        Assert.Equal(new(5, 6), second.Period);
        Assert.Equal(2, course.Fragments.Count);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在不同课程的课程片段上课时间完全重叠时成功
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_DifferentCourseWithSameTime_ShouldSucceed()
    {
        var timetable = CreateTimetable();
        var firstCourseId = AddCourse(timetable, "高等数学", "#FF0000");
        var secondCourseId = AddCourse(timetable, "大学物理", "#00FF00");
        _ = AddSampleFragment(timetable, firstCourseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        var second = AddSampleFragment(timetable, secondCourseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        Assert.Equal(Weekday.Tuesday, second.Weekday);
        Assert.Equal(2, timetable.Courses.Count);
        _ = Assert.Single(timetable.Courses[0].Fragments);
        _ = Assert.Single(timetable.Courses[1].Fragments);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddFragmentToCourse"/> 在课程不存在时返回
    /// <see cref="ErrorCode.CourseNotFound"/>
    /// </summary>
    [Fact]
    public void AddFragmentToCourse_NonExistingCourse_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();

        var result = timetable.AddFragmentToCourse(Guid.NewGuid(), Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.CourseNotFound, failure.Code);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeFragmentWeekday"/> 在改后的上课日期与同一课程的另一课程片段不重叠时成功
    /// </summary>
    [Fact]
    public void ChangeFragmentWeekday_NoOverlap_ShouldSucceed()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        var first = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);
        var second = AddSampleFragment(timetable, courseId, Weekday.Thursday, new(1, 2), [new(1, 16)]);

        var result = timetable.ChangeFragmentWeekday(courseId, second.Id, Weekday.Wednesday);

        _ = Assert.IsType<SuccessResult>(result);
        Assert.Equal(Weekday.Wednesday, second.Weekday);
        Assert.Equal(Weekday.Tuesday, first.Weekday);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeFragmentWeekday"/> 在改后的上课日期与同一课程的另一课程片段的节次重叠时返回
    /// <see cref="ErrorCode.FragmentOverlap"/>, 且不修改原有上课日期
    /// </summary>
    [Fact]
    public void ChangeFragmentWeekday_OverlapsSameCourseFragment_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        _ = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);
        var second = AddSampleFragment(timetable, courseId, Weekday.Thursday, new(1, 2), [new(1, 16)]);

        var result = timetable.ChangeFragmentWeekday(courseId, second.Id, Weekday.Tuesday);

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.FragmentOverlap, failure.Code);
        Assert.Equal(Weekday.Thursday, second.Weekday);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeFragmentWeekday"/> 在改后的上课日期与另一课程片段节次重叠但周次不相交时成功
    /// </summary>
    [Fact]
    public void ChangeFragmentWeekday_OverlapsOnDisjointWeeks_ShouldSucceed()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        _ = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 8)]);
        var second = AddSampleFragment(timetable, courseId, Weekday.Thursday, new(1, 2), [new(9, 16)]);

        var result = timetable.ChangeFragmentWeekday(courseId, second.Id, Weekday.Tuesday);

        _ = Assert.IsType<SuccessResult>(result);
        Assert.Equal(Weekday.Tuesday, second.Weekday);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeFragmentWeekday"/> 在改后的上课日期与原有上课日期相同时成功,
    /// 即判定重叠时排除了课程片段自身
    /// </summary>
    [Fact]
    public void ChangeFragmentWeekday_OverlapsItselfOnly_ShouldSucceed()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        var fragment = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        var result = timetable.ChangeFragmentWeekday(courseId, fragment.Id, Weekday.Tuesday);

        _ = Assert.IsType<SuccessResult>(result);
        Assert.Equal(Weekday.Tuesday, fragment.Weekday);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeFragmentPeriod"/> 在上课节次的结束节号超出课程节次定义数量时返回
    /// <see cref="ErrorCode.PeriodNotFound"/>, 且不修改原有节次
    /// </summary>
    [Fact]
    public void ChangeFragmentPeriod_PeriodEndExceedsPeriodCount_ShouldReturnFailure()
    {
        var timetable = CreateTimetable(periodCount: 12);
        var courseId = AddCourse(timetable);
        var fragment = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        var result = timetable.ChangeFragmentPeriod(courseId, fragment.Id, new(1, 13));

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.PeriodNotFound, failure.Code);
        Assert.Equal(new(1, 2), fragment.Period);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeFragmentPeriod"/> 在改后的节次与同一课程的另一课程片段重叠时返回
    /// <see cref="ErrorCode.FragmentOverlap"/>, 且不修改原有节次
    /// </summary>
    [Fact]
    public void ChangeFragmentPeriod_OverlapsSameCourseFragment_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        _ = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);
        var second = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(3, 4), [new(1, 16)]);

        var result = timetable.ChangeFragmentPeriod(courseId, second.Id, new(2, 3));

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.FragmentOverlap, failure.Code);
        Assert.Equal(new(3, 4), second.Period);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeFragmentPeriod"/> 在改后的节次只与课程片段自身重叠时成功,
    /// 即判定重叠时排除了课程片段自身
    /// </summary>
    [Fact]
    public void ChangeFragmentPeriod_OverlapsItselfOnly_ShouldSucceed()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        var fragment = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        var result = timetable.ChangeFragmentPeriod(courseId, fragment.Id, new(1, 3));

        _ = Assert.IsType<SuccessResult>(result);
        Assert.Equal(new(1, 3), fragment.Period);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeFragmentWeeks"/> 在上课周次列表为空时抛出
    /// <see cref="ArgumentOutOfRangeException"/>
    /// </summary>
    [Fact]
    public void ChangeFragmentWeeks_EmptyWeeks_ShouldThrowArgumentOutOfRangeException()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        var fragment = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => timetable.ChangeFragmentWeeks(courseId, fragment.Id, [])
        );

        Assert.Equal("weeks", ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeFragmentWeeks"/> 在改后的周次与同一课程的另一课程片段重叠时返回
    /// <see cref="ErrorCode.FragmentOverlap"/>, 且不修改原有周次
    /// </summary>
    [Fact]
    public void ChangeFragmentWeeks_OverlapsSameCourseFragment_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        _ = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 8)]);
        var second = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(9, 16)]);

        var result = timetable.ChangeFragmentWeeks(courseId, second.Id, [new(5, 12)]);

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.FragmentOverlap, failure.Code);
        Assert.Equal(new(9, 16), Assert.Single(second.Weeks));
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangeFragmentWeeks"/> 在改后的周次只与课程片段自身重叠时成功,
    /// 即判定重叠时排除了课程片段自身
    /// </summary>
    [Fact]
    public void ChangeFragmentWeeks_OverlapsItselfOnly_ShouldSucceed()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        var fragment = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        var result = timetable.ChangeFragmentWeeks(courseId, fragment.Id, [new(1, 16)]);

        _ = Assert.IsType<SuccessResult>(result);
        Assert.Equal(new(1, 16), Assert.Single(fragment.Weeks));
    }

    /// <summary>
    /// 验证 <see cref="Timetable.RemoveFragmentFromCourse"/> 在课程片段存在时只移除该课程片段,
    /// 同一课程的其他课程片段不受影响
    /// </summary>
    [Fact]
    public void RemoveFragmentFromCourse_ExistingFragment_ShouldRemoveOnlyThatFragment()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        var first = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);
        var second = AddSampleFragment(timetable, courseId, Weekday.Thursday, new(1, 2), [new(1, 16)]);
        var course = Assert.Single(timetable.Courses);

        var result = timetable.RemoveFragmentFromCourse(courseId, first.Id);

        _ = Assert.IsType<SuccessResult>(result);
        var remaining = Assert.Single(course.Fragments);
        Assert.Equal(second.Id, remaining.Id);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.RemoveFragmentFromCourse"/> 在课程不存在时返回
    /// <see cref="ErrorCode.CourseNotFound"/>
    /// </summary>
    [Fact]
    public void RemoveFragmentFromCourse_NonExistingCourse_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();

        var result = timetable.RemoveFragmentFromCourse(Guid.NewGuid(), Guid.NewGuid());

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.CourseNotFound, failure.Code);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.RemoveFragmentFromCourse"/> 在课程片段不存在时返回
    /// <see cref="ErrorCode.FragmentNotFound"/>
    /// </summary>
    [Fact]
    public void RemoveFragmentFromCourse_NonExistingFragment_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();
        var courseId = AddCourse(timetable);
        var course = Assert.Single(timetable.Courses);
        _ = AddSampleFragment(timetable, courseId, Weekday.Tuesday, new(1, 2), [new(1, 16)]);

        var result = timetable.RemoveFragmentFromCourse(courseId, Guid.NewGuid());

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.FragmentNotFound, failure.Code);
        _ = Assert.Single(course.Fragments);
    }
}
