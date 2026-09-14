using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;
using ClassSchedule.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// 仓储测试的共享辅助方法
/// </summary>
internal static class TimetableRepositoryTestHelper
{
    /// <summary>
    /// 测试用的首周周一日期
    /// </summary>
    internal static readonly DateOnly SampleMonday = new(2026, 8, 31);

    /// <summary>
    /// 从操作结果中取出课程表, 失败时使测试失败
    /// </summary>
    /// <param name="result">操作结果</param>
    /// <returns>课程表</returns>
    internal static Timetable ValueOf(Result result)
    {
        return Assert.IsType<SuccessResult<Timetable>>(result).Value;
    }

    /// <summary>
    /// 创建测试用的课程表并保存
    /// </summary>
    /// <param name="repository">课程表仓储</param>
    /// <param name="name">课程表名称</param>
    /// <param name="firstDay">课程表的第一周的任意一天日期</param>
    /// <param name="totalWeeks">课程表总周数</param>
    /// <param name="token">取消令牌</param>
    /// <returns>已保存的课程表</returns>
    internal static async Task<Timetable> CreateTimetableAsync(
        ITimetableRepository repository,
        string name,
        DateOnly firstDay,
        int totalWeeks,
        CancellationToken token)
    {
        var timetable = ValueOf(Timetable.Create(name, firstDay, totalWeeks));
        await repository.AddAsync(timetable, token);
        return timetable;
    }

    /// <summary>
    /// 在已保存的课程表上添加一节课并返回课程与片段的标识
    /// </summary>
    /// <param name="repository">课程表仓储</param>
    /// <param name="timetableId">课程表标识</param>
    /// <param name="token">取消令牌</param>
    /// <returns>课程标识与片段标识</returns>
    internal static async Task<(Guid CourseId, Guid FragmentId)> AddSampleFragmentAsync(
        ITimetableRepository repository,
        Guid timetableId,
        CancellationToken token)
    {
        var timetable = ValueOf(await repository.GetAsync(timetableId, token));
        if (timetable.PeriodDefinitions.Count == 0)
        {
            _ = Assert.IsType<SuccessResult>(timetable.AddPeriodDefinitions((new(8, 0), new(8, 45))));
        }

        var cId = Assert.IsType<SuccessResult<Course>>(timetable.AddCourse("高等数学", "#FF0000")).Value.Id;
        var addResult = timetable.AddFragmentToCourse(
            cId, Weekday.Monday, new(1, 1), [new(1, 16)], "张老师", "教一 101"
        );
        var fId = Assert.IsType<SuccessResult<Fragment>>(addResult).Value.Id;
        await repository.SaveAsync(token);
        return (cId, fId);
    }

    /// <summary>
    /// 在新服务范围中读取指定课程表, 以验证改动是否真的落盘
    /// </summary>
    /// <param name="fixture">测试环境夹具</param>
    /// <param name="timetableId">课程表标识</param>
    /// <param name="token">取消令牌</param>
    /// <returns>课程表</returns>
    internal static async Task<Timetable> ReadInNewScopeAsync(
        TestEnvironmentFixture fixture,
        Guid timetableId,
        CancellationToken token)
    {
        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        return ValueOf(await repository.GetAsync(timetableId, token));
    }

    /// <summary>
    /// 在指定课程表中取出指定片段的周次区间列表
    /// </summary>
    /// <param name="fixture">测试环境夹具</param>
    /// <param name="timetableId">课程表标识</param>
    /// <param name="courseId">课程标识</param>
    /// <param name="fragmentId">片段标识</param>
    /// <param name="token">取消令牌</param>
    /// <returns>周次区间列表</returns>
    internal static async Task<IReadOnlyList<OrdinalRange>> ReadWeeksAsync(
        TestEnvironmentFixture fixture,
        Guid timetableId,
        Guid courseId,
        Guid fragmentId,
        CancellationToken token)
    {
        var timetable = await ReadInNewScopeAsync(fixture, timetableId, token);
        return FindFragment(timetable, courseId, fragmentId).Weeks;
    }

    /// <summary>
    /// 在指定课程表中取出指定片段
    /// </summary>
    /// <param name="timetable">课程表</param>
    /// <param name="courseId">课程标识</param>
    /// <param name="fragmentId">片段标识</param>
    /// <returns>片段</returns>
    internal static Fragment FindFragment(Timetable timetable, Guid courseId, Guid fragmentId)
    {
        var course = Assert.Single(timetable.Courses, c => c.Id == courseId);
        return Assert.Single(course.Fragments, f => f.Id == fragmentId);
    }
}
