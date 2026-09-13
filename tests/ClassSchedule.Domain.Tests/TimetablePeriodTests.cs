using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;

namespace ClassSchedule.Domain.Tests;

/// <summary>
/// <see cref="Timetable"/> 课程节次相关成员的单元测试类
/// </summary>
public sealed class TimetablePeriodTests
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
    /// 验证 <see cref="Timetable.AddPeriodDefinitions"/> 在传入乱序时间时按开始时间升序排列并重新编号
    /// </summary>
    [Fact]
    public void AddPeriodDefinitions_UnsortedTimes_ShouldSortAndRenumber()
    {
        var timetable = CreateTimetable();

        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions(
            (new(10, 0), new(10, 45)),
            (new(8, 0), new(8, 45)),
            (new(9, 0), new(9, 45))
        ));

        Assert.Equal(3, timetable.PeriodDefinitions.Count);
        Assert.Equal(1, timetable.PeriodDefinitions[0].Ordinal);
        Assert.Equal(new(8, 0), timetable.PeriodDefinitions[0].StartTime);
        Assert.Equal(2, timetable.PeriodDefinitions[1].Ordinal);
        Assert.Equal(new(9, 0), timetable.PeriodDefinitions[1].StartTime);
        Assert.Equal(3, timetable.PeriodDefinitions[2].Ordinal);
        Assert.Equal(new(10, 0), timetable.PeriodDefinitions[2].StartTime);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddPeriodDefinitions"/> 在新增的节次定义排在已有节次定义之前时,
    /// 不丢失任何已有节次定义
    /// </summary>
    [Fact]
    public void AddPeriodDefinitions_NewPeriodsBeforeExisting_ShouldNotLoseExistingPeriods()
    {
        var timetable = CreateTimetable();

        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions((new(10, 0), new(10, 45))));
        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions(
            (new(8, 0), new(8, 45)),
            (new(9, 0), new(9, 45))
        ));

        Assert.Equal(3, timetable.PeriodDefinitions.Count);
        Assert.Equal(new(8, 0), timetable.PeriodDefinitions[0].StartTime);
        Assert.Equal(new(9, 0), timetable.PeriodDefinitions[1].StartTime);
        Assert.Equal(new(10, 0), timetable.PeriodDefinitions[2].StartTime);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddPeriodDefinitions"/> 在新增的节次定义排在已有节次定义之后时,
    /// 不丢失任何新增节次定义
    /// </summary>
    [Fact]
    public void AddPeriodDefinitions_NewPeriodsAfterExisting_ShouldNotLoseNewPeriods()
    {
        var timetable = CreateTimetable();

        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions(
            (new(8, 0), new(8, 45)),
            (new(9, 0), new(9, 45))
        ));
        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions((new(10, 0), new(10, 45))));

        Assert.Equal(3, timetable.PeriodDefinitions.Count);
        Assert.Equal(new(8, 0), timetable.PeriodDefinitions[0].StartTime);
        Assert.Equal(new(9, 0), timetable.PeriodDefinitions[1].StartTime);
        Assert.Equal(new(10, 0), timetable.PeriodDefinitions[2].StartTime);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddPeriodDefinitions"/> 在新增后节次定义总数超过
    /// <see cref="Timetable.MaxPeriodDefinitions"/> 时返回 <see cref="ErrorCode.PeriodCountExceedLimit"/>
    /// </summary>
    [Fact]
    public void AddPeriodDefinitions_CountExceedsMaximum_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();
        var periods = new List<(TimeOnly startTime, TimeOnly endTime)>();
        for (var i = 0; i < Timetable.MaxPeriodDefinitions; i++)
        {
            periods.Add((new(8 + i, 0), new(8 + i, 45)));
        }

        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions([.. periods]));

        var result = timetable.AddPeriodDefinitions((new(23, 0), new(23, 45)));

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.PeriodCountExceedLimit, failure.Code);
        Assert.Equal(Timetable.MaxPeriodDefinitions, timetable.PeriodDefinitions.Count);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddPeriodDefinitions"/> 在结束时间不晚于开始时间时抛出
    /// <see cref="ArgumentOutOfRangeException"/>
    /// </summary>
    /// <param name="startHour">开始时间的小时</param>
    /// <param name="startMinute">开始时间的分钟</param>
    /// <param name="endHour">结束时间的小时</param>
    /// <param name="endMinute">结束时间的分钟</param>
    [Theory]
    [InlineData(8, 0, 8, 0)]
    [InlineData(8, 45, 8, 0)]
    public void AddPeriodDefinitions_EndTimeNotAfterStartTime_ShouldThrowArgumentOutOfRangeException(
        int startHour, int startMinute, int endHour, int endMinute)
    {
        var timetable = CreateTimetable();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => timetable.AddPeriodDefinitions((new(startHour, startMinute), new(endHour, endMinute)))
        );

        Assert.Equal("endTime", ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddPeriodDefinitions"/> 在新增的节次定义与已有节次定义时间重叠时返回
    /// <see cref="ErrorCode.PeriodOverlap"/>
    /// </summary>
    [Fact]
    public void AddPeriodDefinitions_TimeOverlapsExisting_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();
        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions((new(8, 0), new(8, 45))));

        var result = timetable.AddPeriodDefinitions((new(8, 30), new(9, 15)));

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.PeriodOverlap, failure.Code);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddPeriodDefinitions"/> 在新增的节次定义与已有节次定义时间紧贴时成功
    /// </summary>
    [Fact]
    public void AddPeriodDefinitions_TimeAdjacentToExisting_ShouldSucceed()
    {
        var timetable = CreateTimetable();

        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions((new(8, 0), new(8, 45))));
        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions((new(8, 45), new(9, 30))));

        Assert.Equal(2, timetable.PeriodDefinitions.Count);
        Assert.Equal(new(8, 45), timetable.PeriodDefinitions[1].StartTime);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.RemovePeriodDefinition"/> 在最后一个节次定义被课程片段占用时返回
    /// <see cref="ErrorCode.PeriodOccupied"/>, 移除该片段后再次移除则成功,
    /// 并将指定序号及其之后的节次定义时间整体前移一格以保持序号连续
    /// </summary>
    [Fact]
    public void RemovePeriodDefinition_LastPeriodOccupied_ShouldFailUntilFragmentRemoved()
    {
        var timetable = CreateTimetable();
        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions(
            (new(8, 0), new(8, 45)),
            (new(9, 0), new(9, 45)),
            (new(10, 0), new(10, 45))
        ));
        var courseId = AddSampleCourse(timetable);
        var addResult = timetable.AddFragmentToCourse(courseId, Weekday.Monday, new(3, 3), [new(1, 16)]);
        var fragmentId = Assert.IsType<SuccessResult<Fragment>>(addResult).Value.Id;

        var blockedResult = timetable.RemovePeriodDefinition(2);

        var failure = Assert.IsType<FailureResult>(blockedResult);
        Assert.Equal(ErrorCode.PeriodOccupied, failure.Code);
        Assert.Equal(3, timetable.PeriodDefinitions.Count);

        _ = Assert.IsType<SuccessResult>(timetable.RemoveFragmentFromCourse(courseId, fragmentId));

        var retryResult = timetable.RemovePeriodDefinition(2);

        _ = Assert.IsType<SuccessResult>(retryResult);
        Assert.Equal(2, timetable.PeriodDefinitions.Count);
        Assert.Equal(1, timetable.PeriodDefinitions[0].Ordinal);
        Assert.Equal(new(8, 0), timetable.PeriodDefinitions[0].StartTime);
        Assert.Equal(2, timetable.PeriodDefinitions[1].Ordinal);
        Assert.Equal(new(10, 0), timetable.PeriodDefinitions[1].StartTime);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.RemovePeriodDefinition"/> 在指定序号超出节次定义数量时返回
    /// <see cref="ErrorCode.PeriodNotFound"/>
    /// </summary>
    [Fact]
    public void RemovePeriodDefinition_OrdinalOutOfRange_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();
        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions((new(8, 0), new(8, 45))));

        var result = timetable.RemovePeriodDefinition(2);

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.PeriodNotFound, failure.Code);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangePeriodDefinitionTime"/> 在修改被引用的节次定义时间时成功
    /// </summary>
    [Fact]
    public void ChangePeriodDefinitionTime_ReferencedPeriod_ShouldSucceed()
    {
        var timetable = CreateTimetable();
        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions(
            (new(8, 0), new(8, 45)),
            (new(9, 0), new(9, 45))
        ));
        var courseId = AddSampleCourse(timetable);
        _ = timetable.AddFragmentToCourse(courseId, Weekday.Monday, new(1, 1), [new(1, 16)]);

        var result = timetable.ChangePeriodDefinitionTime(1, new(8, 10), new(9, 0));

        _ = Assert.IsType<SuccessResult>(result);
        Assert.Equal(new(8, 10), timetable.PeriodDefinitions[0].StartTime);
        Assert.Equal(new(9, 0), timetable.PeriodDefinitions[0].EndTime);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangePeriodDefinitionTime"/> 在修改后与前一个节次定义时间重叠时返回
    /// <see cref="ErrorCode.PeriodOverlap"/>
    /// </summary>
    [Fact]
    public void ChangePeriodDefinitionTime_OverlapsPrevious_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();
        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions(
            (new(8, 0), new(8, 45)),
            (new(9, 0), new(9, 45))
        ));

        var result = timetable.ChangePeriodDefinitionTime(2, new(8, 30), new(9, 45));

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.PeriodOverlap, failure.Code);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangePeriodDefinitionTime"/> 在修改后与后一个节次定义时间重叠时返回
    /// <see cref="ErrorCode.PeriodOverlap"/>
    /// </summary>
    [Fact]
    public void ChangePeriodDefinitionTime_OverlapsNext_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();
        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions(
            (new(8, 0), new(8, 45)),
            (new(9, 0), new(9, 45))
        ));

        var result = timetable.ChangePeriodDefinitionTime(1, new(8, 0), new(9, 10));

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.PeriodOverlap, failure.Code);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.ChangePeriodDefinitionTime"/> 在指定序号超出节次定义数量时返回
    /// <see cref="ErrorCode.PeriodNotFound"/>
    /// </summary>
    [Fact]
    public void ChangePeriodDefinitionTime_OrdinalOutOfRange_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();
        _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions((new(8, 0), new(8, 45))));

        var result = timetable.ChangePeriodDefinitionTime(2, new(9, 0), new(9, 45));

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.PeriodNotFound, failure.Code);
    }
}
