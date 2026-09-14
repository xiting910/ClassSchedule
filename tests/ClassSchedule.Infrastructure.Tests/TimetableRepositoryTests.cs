using ClassSchedule.Domain.Models;
using ClassSchedule.Infrastructure.Interfaces;
using ClassSchedule.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// <see cref="ITimetableRepository"/> 的单元测试类
/// </summary>
/// <param name="fixture">测试环境夹具</param>
public sealed class TimetableRepositoryTests(TestEnvironmentFixture fixture)
{
    /// <summary>
    /// 验证列表按首周周一升序返回课程表摘要, 且摘要字段与保存的内容一致
    /// </summary>
    [Fact]
    public async Task ListAsync_TwoTimetables_ShouldReturnSummariesOrderedByFirstMonday()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var later = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "较晚的课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );
        var earlier = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "较早的课表", new(2026, 3, 2), 18, token
        );

        var summaries = await repository.ListAsync(token);

        var laterIndex = IndexOf(summaries, later.Id);
        var earlierIndex = IndexOf(summaries, earlier.Id);
        Assert.True(earlierIndex < laterIndex);
        Assert.Equal(new(2026, 3, 2), summaries[earlierIndex].FirstMonday);
        Assert.Equal(18, summaries[earlierIndex].TotalWeeks);
        Assert.Equal("较早的课表", summaries[earlierIndex].Name);
    }

    /// <summary>
    /// 验证查询会加载完整的聚合根, 即课程及其片段随课程表一并返回且字段完整
    /// </summary>
    [Fact]
    public async Task GetAsync_ExistingTimetable_ShouldLoadWholeAggregate()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var timetable = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "聚合加载课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );
        var (courseId, fragmentId) = await TimetableRepositoryTestHelper.AddSampleFragmentAsync(
            repository, timetable.Id, token
        );

        var loaded = TimetableRepositoryTestHelper.ValueOf(await repository.GetAsync(timetable.Id, token));

        var course = Assert.Single(loaded.Courses);
        Assert.Equal(courseId, course.Id);
        Assert.Equal("高等数学", course.Name);
        Assert.Equal("#FF0000", course.Color);
        var fragment = Assert.Single(course.Fragments);
        Assert.Equal(fragmentId, fragment.Id);
        Assert.Equal("张老师", fragment.Teacher);
        Assert.Equal("教一 101", fragment.Location);
        Assert.Equal(Weekday.Monday, fragment.Weekday);
        Assert.Equal(new(1, 1), fragment.Period);
    }

    /// <summary>
    /// 验证 <see cref="ITimetableRepository.GetAsync"/> 在课程表不存在时返回
    /// <see cref="ErrorCode.TimetableNotFound"/>
    /// </summary>
    [Fact]
    public async Task GetAsync_MissingTimetable_ShouldReturnTimetableNotFound()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();

        var result = await repository.GetAsync(Guid.NewGuid(), token);

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.TimetableNotFound, failure.Code);
    }

    /// <summary>
    /// 验证 <see cref="ITimetableRepository.AddAsync"/> 保存的课程表可由新范围读回
    /// </summary>
    [Fact]
    public async Task AddAsync_NewTimetable_ShouldPersistIt()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var timetable = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "新增课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );

        var loaded = await TimetableRepositoryTestHelper.ReadInNewScopeAsync(fixture, timetable.Id, token);

        Assert.Equal(timetable.Id, loaded.Id);
        Assert.Equal("新增课表", loaded.Name);
        Assert.Equal(16, loaded.TotalWeeks);
    }

    /// <summary>
    /// 验证 <see cref="ITimetableRepository.DeleteAsync"/> 删除存在的课程表并返回成功
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ExistingTimetable_ShouldRemoveIt()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var timetable = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "待删除课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );

        var result = await repository.DeleteAsync(timetable.Id, token);

        _ = Assert.IsType<SuccessResult>(result);
        var reloaded = await repository.GetAsync(timetable.Id, token);
        var failure = Assert.IsType<FailureResult>(reloaded);
        Assert.Equal(ErrorCode.TimetableNotFound, failure.Code);
    }

    /// <summary>
    /// 验证 <see cref="ITimetableRepository.DeleteAsync"/> 在课程表不存在时返回
    /// <see cref="ErrorCode.TimetableNotFound"/>
    /// </summary>
    [Fact]
    public async Task DeleteAsync_MissingTimetable_ShouldReturnTimetableNotFound()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();

        var result = await repository.DeleteAsync(Guid.NewGuid(), token);

        var failure = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.TimetableNotFound, failure.Code);
    }

    /// <summary>
    /// 验证 <see cref="ITimetableRepository.SaveAsync"/> 在没有改动时成功返回, 即零影响行数不算失败
    /// </summary>
    [Fact]
    public async Task SaveAsync_NoChanges_ShouldSucceed()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        _ = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "无改动课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );

        await repository.SaveAsync(token);
    }

    /// <summary>
    /// 查找指定课程表在摘要列表中的索引
    /// </summary>
    /// <param name="summaries">课程表摘要列表</param>
    /// <param name="timetableId">课程表标识</param>
    /// <returns>索引, 未找到时为 -1</returns>
    private static int IndexOf(IReadOnlyList<TimetableSummary> summaries, Guid timetableId)
    {
        for (var index = 0; index < summaries.Count; index++)
        {
            if (summaries[index].Id == timetableId)
            {
                return index;
            }
        }

        return -1;
    }
}
