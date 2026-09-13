using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;

namespace ClassSchedule.Domain.Tests;

/// <summary>
/// <see cref="Timetable"/> 课程相关成员的单元测试类
/// </summary>
public sealed class TimetableCourseTests
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
    /// 验证 <see cref="Timetable.AddCourse"/> 在课程表中新增一门片段数为零的课程
    /// </summary>
    [Fact]
    public void AddCourse_ShouldAddCourseWithNoFragments()
    {
        var timetable = CreateTimetable();

        var result = timetable.AddCourse("高等数学", "#FF0000", "必修");

        var course = Assert.IsType<SuccessResult<Course>>(result).Value;
        Assert.Equal("高等数学", course.Name);
        Assert.Equal("#FF0000", course.Color);
        Assert.Equal("必修", course.Note);
        Assert.Empty(course.Fragments);
        _ = Assert.Single(timetable.Courses);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.AddCourse"/> 在课程名或颜色为空白时抛出 <see cref="ArgumentException"/>
    /// </summary>
    [Fact]
    public void AddCourse_BlankNameOrColor_ShouldThrowArgumentException()
    {
        var timetable = CreateTimetable();

        _ = Assert.Throws<ArgumentException>(() => timetable.AddCourse("", "#FF0000"));
        _ = Assert.Throws<ArgumentException>(() => timetable.AddCourse("高等数学", "   "));
    }

    /// <summary>
    /// 验证 <see cref="Timetable.RemoveCourse"/> 在课程存在时移除该课程
    /// </summary>
    [Fact]
    public void RemoveCourse_ExistingCourse_ShouldRemoveCourse()
    {
        var timetable = CreateTimetable();
        var courseId = AddSampleCourse(timetable);

        var result = timetable.RemoveCourse(courseId);

        _ = Assert.IsType<SuccessResult>(result);
        Assert.Empty(timetable.Courses);
    }

    /// <summary>
    /// 验证 <see cref="Timetable.RemoveCourse"/> 在课程不存在时返回 <see cref="ErrorCode.CourseNotFound"/>
    /// </summary>
    [Fact]
    public void RemoveCourse_NonExistingCourse_ShouldReturnFailure()
    {
        var timetable = CreateTimetable();

        var result = timetable.RemoveCourse(Guid.NewGuid());

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.CourseNotFound, failure.Code);
    }
}
