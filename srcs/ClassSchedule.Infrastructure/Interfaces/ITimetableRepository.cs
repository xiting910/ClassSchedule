using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;
using ClassSchedule.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClassSchedule.Infrastructure.Interfaces;

/// <summary>
/// 课程表仓储接口
/// </summary>
public interface ITimetableRepository
{
    /// <summary>
    /// 获取课程表摘要列表
    /// </summary>
    /// <param name="token">可选的取消令牌</param>
    /// <returns>包含课程表摘要的只读列表</returns>
    Task<IReadOnlyList<TimetableSummary>> ListAsync(CancellationToken token = default);

    /// <summary>
    /// 获取指定课程表
    /// </summary>
    /// <remarks>
    /// 注意: 对成功结果携带的实例的修改必须由同一仓储实例的 <see cref="SaveAsync"/> 提交
    /// </remarks>
    /// <param name="timetableId">课程表的唯一标识符</param>
    /// <param name="token">可选的取消令牌</param>
    /// <returns>操作结果, 成功时包含指定的课程表实例</returns>
    Task<Result> GetAsync(Guid timetableId, CancellationToken token = default);

    /// <summary>
    /// 删除指定课程表
    /// </summary>
    /// <param name="timetableId">课程表的唯一标识符</param>
    /// <param name="token">可选的取消令牌</param>
    /// <returns>操作结果</returns>
    Task<Result> DeleteAsync(Guid timetableId, CancellationToken token = default);

    /// <summary>
    /// 添加新的课程表
    /// </summary>
    /// <param name="timetable">要添加的课程表实例</param>
    /// <param name="token">可选的取消令牌</param>
    Task AddAsync(Timetable timetable, CancellationToken token = default);

    /// <summary>
    /// 保存课程表的更改
    /// </summary>
    /// <param name="token">可选的取消令牌</param>
    Task SaveAsync(CancellationToken token = default);
}
