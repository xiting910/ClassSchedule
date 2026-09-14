using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;
using ClassSchedule.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// 课程表聚合改动的落盘测试类, 覆盖取回实例、修改聚合根、提交改动的完整链路
/// </summary>
/// <param name="fixture">测试环境夹具</param>
public sealed class TimetableRepositoryPersistenceTests(TestEnvironmentFixture fixture)
{
    /// <summary>
    /// 验证两张课程表可以各自拥有第 1 节课程节次定义, 即课程节次定义的主键包含课程表标识
    /// </summary>
    [Fact]
    public async Task AddPeriodDefinitions_TwoTimetablesEachWithFirstOrdinal_ShouldBothPersist()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var first = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "第一张课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );
        var second = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "第二张课表", new(2026, 3, 2), 16, token
        );

        var firstLoaded = TimetableRepositoryTestHelper.ValueOf(await repository.GetAsync(first.Id, token));
        _ = Assert.IsType<SuccessResult>(firstLoaded.AddPeriodDefinitions((new(8, 0), new(8, 45))));
        await repository.SaveAsync(token);

        var secondLoaded = TimetableRepositoryTestHelper.ValueOf(await repository.GetAsync(second.Id, token));
        _ = Assert.IsType<SuccessResult>(secondLoaded.AddPeriodDefinitions((new(8, 0), new(8, 45))));
        await repository.SaveAsync(token);

        first = await TimetableRepositoryTestHelper.ReadInNewScopeAsync(fixture, first.Id, token);
        second = await TimetableRepositoryTestHelper.ReadInNewScopeAsync(fixture, second.Id, token);
        Assert.Equal(new(8, 0), Assert.Single(first.PeriodDefinitions).StartTime);
        Assert.Equal(new(8, 0), Assert.Single(second.PeriodDefinitions).StartTime);
    }

    /// <summary>
    /// 验证对 <see cref="ITimetableRepository.GetAsync"/> 返回的实例所做的聚合根改动,
    /// 由同一仓储实例的 <see cref="ITimetableRepository.SaveAsync"/> 落盘
    /// </summary>
    [Fact]
    public async Task GetAsync_ModifyAggregateThenSave_ShouldPersistChanges()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var timetable = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "改动落盘课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );
        var loaded = TimetableRepositoryTestHelper.ValueOf(await repository.GetAsync(timetable.Id, token));

        loaded.Name = "改动后的课表名称";
        _ = Assert.IsType<SuccessResult>(loaded.ChangeTotalWeeks(20));
        await repository.SaveAsync(token);

        var reloaded = await TimetableRepositoryTestHelper.ReadInNewScopeAsync(fixture, timetable.Id, token);
        Assert.Equal("改动后的课表名称", reloaded.Name);
        Assert.Equal(20, reloaded.TotalWeeks);
    }

    /// <summary>
    /// 验证移除课程节次定义时, 后续节次的时间整体前移并由更新语句落盘
    /// </summary>
    [Fact]
    public async Task RemovePeriodDefinition_ShiftsFollowingTimes_ShouldPersistWithoutBreakingReferences()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var timetable = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "节次移除课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );
        var prepared = TimetableRepositoryTestHelper.ValueOf(await repository.GetAsync(timetable.Id, token));
        _ = Assert.IsType<SuccessResult>(prepared.AddPeriodDefinitions(
            (new(8, 0), new(8, 45)),
            (new(9, 0), new(9, 45)),
            (new(10, 0), new(10, 45))
        ));
        await repository.SaveAsync(token);

        var loaded = TimetableRepositoryTestHelper.ValueOf(await repository.GetAsync(timetable.Id, token));
        var courseId = Assert.IsType<SuccessResult<Course>>(
            loaded.AddCourse("高等数学", "#FF0000")
        ).Value.Id;
        _ = Assert.IsType<SuccessResult<Fragment>>(
            loaded.AddFragmentToCourse(courseId, Weekday.Monday, new(2, 2), [new(1, 16)])
        );
        await repository.SaveAsync(token);

        _ = Assert.IsType<SuccessResult>(loaded.RemovePeriodDefinition(1));
        await repository.SaveAsync(token);

        var reloaded = await TimetableRepositoryTestHelper.ReadInNewScopeAsync(fixture, timetable.Id, token);
        Assert.Equal([1, 2], reloaded.PeriodDefinitions.Select(p => p.Ordinal));
        Assert.Equal(new(9, 0), reloaded.PeriodDefinitions[0].StartTime);
        Assert.Equal(new(9, 45), reloaded.PeriodDefinitions[0].EndTime);
        Assert.Equal(new(10, 0), reloaded.PeriodDefinitions[1].StartTime);
        Assert.Equal(new(2, 2), Assert.Single(Assert.Single(reloaded.Courses).Fragments).Period);
    }
}
