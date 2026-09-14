using ClassSchedule.Infrastructure.Interfaces;
using ClassSchedule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// 实体物化拦截器的测试类
/// </summary>
/// <param name="fixture">测试环境夹具</param>
public sealed class MaterializationInterceptorTests(TestEnvironmentFixture fixture)
{
    /// <summary>
    /// 验证物化拦截器在实体物化后按序号升序排列课程节次定义, 即绕过实体写入路径插入的乱序行在读取时被纠正
    /// </summary>
    [Fact]
    public async Task GetAsync_PeriodDefinitionsStoredOutOfOrder_ShouldSortAfterMaterialization()
    {
        var token = TestContext.Current.CancellationToken;

        using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITimetableRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var timetable = await TimetableRepositoryTestHelper.CreateTimetableAsync(
            repository, "节次排序课表", TimetableRepositoryTestHelper.SampleMonday, 16, token
        );
        _ = await dbContext.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO PeriodDefinition (TimetableId, Ordinal, StartTime, EndTime) VALUES
            ({0}, 2, '09:00:00', '09:45:00'),
            ({0}, 1, '08:00:00', '08:45:00')
            """,
            [timetable.Id],
            token
        );

        var loaded = await TimetableRepositoryTestHelper.ReadInNewScopeAsync(fixture, timetable.Id, token);

        Assert.Equal([1, 2], loaded.PeriodDefinitions.Select(p => p.Ordinal));
        Assert.Equal(new(8, 0), loaded.PeriodDefinitions[0].StartTime);
        Assert.Equal(new(9, 0), loaded.PeriodDefinitions[1].StartTime);
    }
}
