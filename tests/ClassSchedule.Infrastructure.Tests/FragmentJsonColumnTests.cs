using ClassSchedule.Domain.Models;
using ClassSchedule.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// 课程片段的 JSON 单列持久化测试类, 覆盖节次区间与周次区间的往返与变更检测
/// </summary>
/// <param name="fixture">测试环境夹具</param>
public sealed class FragmentJsonColumnTests(TestEnvironmentFixture fixture)
{
    /// <summary>
    /// 验证课程片段的周次区间以 JSON 存储后往返一致, 即元素与顺序均不丢失
    /// </summary>
    [Fact]
    public async Task Weeks_RoundTrip_ShouldPreserveAllRanges()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var timetable = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "周次往返课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );
        var (courseId, fragmentId) = await TimetableRepositoryTestHelper.AddSampleFragmentAsync(
            repository, timetable.Id, token
        );

        var weeks = await TimetableRepositoryTestHelper.ReadWeeksAsync(
            fixture, timetable.Id, courseId, fragmentId, token
        );

        Assert.Equal([new(1, 16)], weeks);
    }

    /// <summary>
    /// 验证只包含一个区间的周次列表往返一致, 覆盖列表元素少于两个时的归一化分支
    /// </summary>
    [Fact]
    public async Task Weeks_SingleRange_ShouldRoundTrip()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var timetable = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "单区间课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );
        var (courseId, fragmentId) = await TimetableRepositoryTestHelper.AddSampleFragmentAsync(
            repository, timetable.Id, token
        );
        var loaded = TimetableRepositoryTestHelper.ValueOf(await repository.GetAsync(timetable.Id, token));

        _ = Assert.IsType<SuccessResult>(loaded.ChangeFragmentWeeks(courseId, fragmentId, [new(5, 5)]));
        await repository.SaveAsync(token);

        var weeks = await TimetableRepositoryTestHelper.ReadWeeksAsync(
            fixture, timetable.Id, courseId, fragmentId, token
        );
        Assert.Equal([new(5, 5)], weeks);
    }

    /// <summary>
    /// 验证相邻区间在归一化时被合并, 且合并后的形态被完整落盘
    /// </summary>
    [Fact]
    public async Task Weeks_AdjacentRanges_ShouldMergeAndPersist()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var timetable = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "相邻合并课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );
        var (courseId, fragmentId) = await TimetableRepositoryTestHelper.AddSampleFragmentAsync(
            repository, timetable.Id, token
        );
        var loaded = TimetableRepositoryTestHelper.ValueOf(await repository.GetAsync(timetable.Id, token));

        _ = Assert.IsType<SuccessResult>(
            loaded.ChangeFragmentWeeks(courseId, fragmentId, [new(1, 8), new(9, 16)])
        );
        await repository.SaveAsync(token);

        var weeks = await TimetableRepositoryTestHelper.ReadWeeksAsync(
            fixture, timetable.Id, courseId, fragmentId, token
        );
        Assert.Equal([new(1, 16)], weeks);
    }

    /// <summary>
    /// 验证修改周次区间后由 <see cref="ITimetableRepository.SaveAsync"/> 落盘,
    /// 即周次列表被识别为已修改而不是与快照相等
    /// </summary>
    [Fact]
    public async Task ChangeFragmentWeeks_AfterSave_ShouldPersistNewRanges()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var timetable = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "周次修改课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );
        var (courseId, fragmentId) = await TimetableRepositoryTestHelper.AddSampleFragmentAsync(
            repository, timetable.Id, token
        );
        var loaded = TimetableRepositoryTestHelper.ValueOf(await repository.GetAsync(timetable.Id, token));

        _ = Assert.IsType<SuccessResult>(
            loaded.ChangeFragmentWeeks(courseId, fragmentId, [new(1, 8), new(10, 16)])
        );
        await repository.SaveAsync(token);

        var weeks = await TimetableRepositoryTestHelper.ReadWeeksAsync(
            fixture, timetable.Id, courseId, fragmentId, token
        );
        Assert.Equal([new(1, 8), new(10, 16)], weeks);
    }

    /// <summary>
    /// 验证修改片段的节次区间后由 <see cref="ITimetableRepository.SaveAsync"/> 落盘
    /// </summary>
    [Fact]
    public async Task ChangeFragmentPeriod_AfterSave_ShouldPersistNewPeriod()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var timetable = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "节次修改课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );
        var (courseId, fragmentId) = await TimetableRepositoryTestHelper.AddSampleFragmentAsync(
            repository, timetable.Id, token
        );
        var loaded = TimetableRepositoryTestHelper.ValueOf(await repository.GetAsync(timetable.Id, token));

        _ = Assert.IsType<SuccessResult>(loaded.ChangeFragmentPeriod(courseId, fragmentId, new(1, 1)));
        await repository.SaveAsync(token);

        var reloaded = await TimetableRepositoryTestHelper.ReadInNewScopeAsync(fixture, timetable.Id, token);
        var fragment = TimetableRepositoryTestHelper.FindFragment(reloaded, courseId, fragmentId);
        Assert.Equal(new(1, 1), fragment.Period);
    }
}
