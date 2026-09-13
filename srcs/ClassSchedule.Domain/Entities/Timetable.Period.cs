using ClassSchedule.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ClassSchedule.Domain.Entities;

// Timetable 的分部类, 用于定义课程节次相关的操作
#pragma warning disable IDE0040 // 分部类不需要可访问性修饰符
partial class Timetable
#pragma warning restore IDE0040 // 分部类不需要可访问性修饰符
{
    /// <summary>
    /// 添加课程节次定义
    /// </summary>
    /// <param name="periods">课程节次定义的开始时间和结束时间元组列表</param>
    /// <returns>操作结果</returns>
    /// <exception cref="ArgumentNullException">参数 为 <see langword="null"/> 时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">参数不满足约束时抛出</exception>
    public Result AddPeriodDefinitions(params IEnumerable<(TimeOnly startTime, TimeOnly endTime)> periods)
    {
        ArgumentNullException.ThrowIfNull(periods, nameof(periods));

        var ordinal = 1;
        var lastEndTime = TimeOnly.MinValue;
        List<PeriodDefinition> newDefinitions = [];
        foreach (var (startTime, endTime) in periods.OrderBy(p => p.startTime))
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(endTime, startTime, nameof(endTime));
            ArgumentOutOfRangeException.ThrowIfLessThan(startTime, lastEndTime, nameof(startTime));
            newDefinitions.Add(new() { Ordinal = ordinal++, StartTime = startTime, EndTime = endTime });
            lastEndTime = endTime;
        }

        if (newDefinitions.Count == 0)
        {
            return Result.Success();
        }

        if (_periodDefinitions.Count + newDefinitions.Count > MaxPeriodDefinitions)
        {
            return Result.Failure(
                ErrorCode.PeriodCountExceedLimit,
                $"课程节次定义总数不能超过 {MaxPeriodDefinitions} 个, 当前已有 {_periodDefinitions.Count} 个, 新增 {newDefinitions.Count} 个将超过限制"
            );
        }

        if (_periodDefinitions.Count == 0)
        {
            _periodDefinitions = newDefinitions;
            return Result.Success();
        }

        ordinal = 1;
        lastEndTime = TimeOnly.MinValue;
        int i = 0, j = 0;
        bool currentIsNew;
        PeriodDefinition exist, newDef, merged;
        List<PeriodDefinition> mergedDefinitions = [];
        while (i < _periodDefinitions.Count && j < newDefinitions.Count)
        {
            exist = _periodDefinitions[i];
            newDef = newDefinitions[j];

            if (exist.StartTime < newDef.StartTime)
            {
                merged = exist.Ordinal == ordinal ? exist : new()
                {
                    Ordinal = ordinal++,
                    StartTime = exist.StartTime,
                    EndTime = exist.EndTime
                };
                currentIsNew = false;
                i++;
            }
            else
            {
                merged = newDef.Ordinal == ordinal ? newDef : new()
                {
                    Ordinal = ordinal++,
                    StartTime = newDef.StartTime,
                    EndTime = newDef.EndTime
                };
                currentIsNew = true;
                j++;
            }

            if (merged.StartTime < lastEndTime)
            {
                return Result.Failure(
                    ErrorCode.PeriodOverlap,
                    currentIsNew
                        ? $"新添加的课程节次定义 {merged.StartTime}-{merged.EndTime} 与已存在的课程节次定义 {mergedDefinitions[^1].StartTime}-{lastEndTime} 冲突"
                        : $"已存在的课程节次定义 {merged.StartTime}-{merged.EndTime} 与新添加的课程节次定义 {mergedDefinitions[^1].StartTime}-{lastEndTime} 冲突"
                );
            }

            mergedDefinitions.Add(merged);
            lastEndTime = merged.EndTime;
        }
        while (i < _periodDefinitions.Count)
        {
            exist = _periodDefinitions[i++];
            merged = exist.Ordinal == ordinal ? exist : new()
            {
                Ordinal = ordinal++,
                StartTime = exist.StartTime,
                EndTime = exist.EndTime
            };
            if (merged.StartTime < lastEndTime)
            {
                return Result.Failure(
                    ErrorCode.PeriodOverlap,
                    $"已存在的课程节次定义 {merged.StartTime}-{merged.EndTime} 与新添加的课程节次定义 {mergedDefinitions[^1].StartTime}-{lastEndTime} 冲突"
                );
            }
            mergedDefinitions.Add(merged);
            lastEndTime = merged.EndTime;
        }
        while (j < newDefinitions.Count)
        {
            newDef = newDefinitions[j++];
            merged = newDef.Ordinal == ordinal ? newDef : new()
            {
                Ordinal = ordinal++,
                StartTime = newDef.StartTime,
                EndTime = newDef.EndTime
            };
            if (merged.StartTime < lastEndTime)
            {
                return Result.Failure(
                    ErrorCode.PeriodOverlap,
                    $"新添加的课程节次定义 {merged.StartTime}-{merged.EndTime} 与已存在的课程节次定义 {mergedDefinitions[^1].StartTime}-{lastEndTime} 冲突"
                );
            }
            mergedDefinitions.Add(merged);
            lastEndTime = merged.EndTime;
        }

        _periodDefinitions = mergedDefinitions;
        return Result.Success();
    }

    /// <summary>
    /// 移除课程节次定义
    /// </summary>
    /// <remarks><para>
    /// 该方法实际移除的是最后一个课程节次定义, 并将指定序号及之后的课程节次定义的时间设置为其后一个课程节次定义的时间
    /// </para><para>
    /// 因此, 如果最后一个课程节次定义被某个课程占用, 则无法进行移除操作
    /// </para></remarks>
    /// <param name="ordinal">课程节次定义的序号</param>
    /// <returns>操作结果</returns>
    /// <exception cref="ArgumentOutOfRangeException">参数不满足约束时抛出</exception>
    public Result RemovePeriodDefinition(int ordinal)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ordinal, nameof(ordinal));

        if (ordinal > _periodDefinitions.Count)
        {
            return Result.Failure(ErrorCode.PeriodNotFound, $"序号为 {ordinal} 的课程节次定义不存在");
        }

        var lastOrdinal = _periodDefinitions.Count;
        Debug.Assert(_periodDefinitions[lastOrdinal - 1].Ordinal == lastOrdinal, "课程节次定义序号不连续");
        foreach (var course in _courses)
        {
            if (course._fragments.Any(f => f.Period.Contains(lastOrdinal)))
            {
                return Result.Failure(
                    ErrorCode.PeriodOccupied,
                    $"课程 {course.Name} 有节次为 {lastOrdinal} 的上课时间, 无法移除课程节次定义"
                );
            }
        }

        var index = ordinal - 1;
        Debug.Assert(_periodDefinitions[index].Ordinal == ordinal, "课程节次定义序号不连续");
        while (index < _periodDefinitions.Count - 1)
        {
            _periodDefinitions[index].StartTime = _periodDefinitions[index + 1].StartTime;
            _periodDefinitions[index].EndTime = _periodDefinitions[index + 1].EndTime;
            index++;
        }
        _periodDefinitions.RemoveAt(index);
        return Result.Success();
    }

    /// <summary>
    /// 更改指定课程节次定义的时间
    /// </summary>
    /// <param name="ordinal">课程节次定义的序号</param>
    /// <param name="startTime">新的开始时间</param>
    /// <param name="endTime">新的结束时间</param>
    /// <returns>操作结果</returns>
    /// <exception cref="ArgumentOutOfRangeException">参数不满足约束时抛出</exception>
    public Result ChangePeriodDefinitionTime(int ordinal, TimeOnly startTime, TimeOnly endTime)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ordinal, nameof(ordinal));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(endTime, startTime, nameof(endTime));

        if (ordinal > _periodDefinitions.Count)
        {
            return Result.Failure(ErrorCode.PeriodNotFound, $"序号为 {ordinal} 的课程节次定义不存在");
        }

        var index = ordinal - 1;
        Debug.Assert(_periodDefinitions[index].Ordinal == ordinal, "课程节次定义序号不连续");

        if (index > 0)
        {
            var prevEndTime = _periodDefinitions[index - 1].EndTime;
            if (startTime < prevEndTime)
            {
                return Result.Failure(
                    ErrorCode.PeriodOverlap,
                    $"课程节次定义 {ordinal} 的新开始时间 {startTime} 与前一个课程节次定义的结束时间 {prevEndTime} 冲突"
                );
            }
        }
        if (index < _periodDefinitions.Count - 1)
        {
            var nextStartTime = _periodDefinitions[index + 1].StartTime;
            if (endTime > nextStartTime)
            {
                return Result.Failure(
                    ErrorCode.PeriodOverlap,
                    $"课程节次定义 {ordinal} 的新结束时间 {endTime} 与后一个课程节次定义的开始时间 {nextStartTime} 冲突"
                );
            }
        }

        _periodDefinitions[index].StartTime = startTime;
        _periodDefinitions[index].EndTime = endTime;
        return Result.Success();
    }
}
