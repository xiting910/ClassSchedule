using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;
using ClassSchedule.Infrastructure.Interfaces;
using ClassSchedule.Infrastructure.Models;
using ClassSchedule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClassSchedule.Infrastructure.Services;

/// <summary>
/// <see cref="ITimetableRepository"/> 的实现, 提供课程表数据的持久化存储和访问
/// </summary>
/// <param name="_logger">日志记录器</param>
/// <param name="_dbContext">数据库上下文</param>
internal sealed partial class TimetableRepository(
    ILogger<TimetableRepository> _logger,
    AppDbContext _dbContext
) : ITimetableRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<TimetableSummary>> ListAsync(CancellationToken token = default)
    {
        var summaries = await _dbContext.Timetables
            .AsNoTracking()
            .OrderBy(t => t.FirstMonday)
            .ThenBy(t => t.Name)
            .Select(t => new TimetableSummary(t.Id, t.Name, t.FirstMonday, t.TotalWeeks))
            .ToListAsync(token)
            .ConfigureAwait(false);

        LogListed(summaries.Count);
        return summaries;
    }

    /// <inheritdoc/>
    public async Task<Result> GetAsync(Guid timetableId, CancellationToken token = default)
    {
        var timetable = await _dbContext.Timetables.FindAsync([timetableId], token).ConfigureAwait(false);
        if (timetable is null)
        {
            LogNotFound(timetableId);
            return Result.Failure(ErrorCode.TimetableNotFound, $"课程表 {timetableId} 不存在");
        }

        return Result.Success(timetable);
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(Guid timetableId, CancellationToken token = default)
    {
        var timetable = await _dbContext.Timetables.FindAsync([timetableId], token).ConfigureAwait(false);
        if (timetable is null)
        {
            LogNotFound(timetableId);
            return Result.Failure(ErrorCode.TimetableNotFound, $"课程表 {timetableId} 不存在");
        }

        _ = _dbContext.Timetables.Remove(timetable);
        var affectedRows = await _dbContext.SaveChangesAsync(token).ConfigureAwait(false);

        LogDeleted(timetableId, affectedRows);
        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task AddAsync(Timetable timetable, CancellationToken token = default)
    {
        _ = await _dbContext.Timetables.AddAsync(timetable, token).ConfigureAwait(false);
        var affectedRows = await _dbContext.SaveChangesAsync(token).ConfigureAwait(false);

        LogAdded(timetable.Id, timetable.Name, affectedRows);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(CancellationToken token = default)
    {
        var affectedRows = await _dbContext.SaveChangesAsync(token).ConfigureAwait(false);
        LogSaved(affectedRows);
    }

    /// <summary>
    /// 记录课程表列表加载完成的日志
    /// </summary>
    /// <param name="count">加载的课程表数量</param>
    [LoggerMessage(
        EventId = 1,
        EventName = "Listed",
        Level = LogLevel.Debug,
        Message = "Timetable list loaded: {Count} items"
    )]
    private partial void LogListed(int count);

    /// <summary>
    /// 记录课程表未找到的日志
    /// </summary>
    /// <param name="timetableId">课程表 Id</param>
    [LoggerMessage(
        EventId = 2,
        EventName = "NotFound",
        Level = LogLevel.Warning,
        Message = "Timetable not found: {TimetableId}"
    )]
    private partial void LogNotFound(Guid timetableId);

    /// <summary>
    /// 记录课程表已删除的日志
    /// </summary>
    /// <param name="timetableId">课程表 Id</param>
    /// <param name="affectedRows">受影响的行数</param>
    [LoggerMessage(
        EventId = 3,
        EventName = "Deleted",
        Level = LogLevel.Information,
        Message = "Timetable deleted: {TimetableId}, AffectedRows={AffectedRows}"
    )]
    private partial void LogDeleted(Guid timetableId, int affectedRows);

    /// <summary>
    /// 记录课程表已新增的日志
    /// </summary>
    /// <param name="timetableId">课程表 Id</param>
    /// <param name="name">课程表名称</param>
    /// <param name="affectedRows">受影响的行数</param>
    [LoggerMessage(
        EventId = 4,
        EventName = "Added",
        Level = LogLevel.Information,
        Message = "Timetable added: {TimetableId}, Name={Name}, AffectedRows={AffectedRows}"
    )]
    private partial void LogAdded(Guid timetableId, string name, int affectedRows);

    /// <summary>
    /// 记录课程表更改已保存的日志
    /// </summary>
    /// <param name="affectedRows">受影响的行数</param>
    [LoggerMessage(
        EventId = 5,
        EventName = "Saved",
        Level = LogLevel.Information,
        Message = "Timetable changes saved: AffectedRows={AffectedRows}"
    )]
    private partial void LogSaved(int affectedRows);
}
