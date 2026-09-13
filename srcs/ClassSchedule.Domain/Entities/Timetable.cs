using ClassSchedule.Domain.Models;
using System;
using System.Collections.Generic;

namespace ClassSchedule.Domain.Entities;

/// <summary>
/// 表示一个完整的课程表
/// </summary>
public sealed partial class Timetable : IEquatable<Timetable>
{
    /// <summary>
    /// 课程表节次定义的最大数量
    /// </summary>
    public const int MaxPeriodDefinitions = 15;

    /// <summary>
    /// 课程表总周数的最大值
    /// </summary>
    public const int MaxTotalWeeks = 30;

    /// <summary>
    /// 唯一标识符
    /// </summary>
    public Guid Id { get; private init; }

    /// <summary>
    /// 课程表名称
    /// </summary>
    /// <exception cref="ArgumentException">参数 为 <see langword="null"/> 或空白字符时抛出</exception>
    public required string Name
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Name));
            field = value;
        }
    }

    /// <summary>
    /// 课程表的第一周的星期一日期
    /// </summary>
    public DateOnly FirstMonday
    {
        get;
        set => field = value.GetMonday();
    }

    /// <summary>
    /// 课程表总周数
    /// </summary>
    public int TotalWeeks { get; private set; }

    /// <summary>
    /// 课程表的课程节次定义只读列表
    /// </summary>
    public IReadOnlyList<PeriodDefinition> PeriodDefinitions => _periodDefinitions;

    /// <summary>
    /// 课程表的课程只读列表
    /// </summary>
    public IReadOnlyList<Course> Courses => _courses;

    /// <summary>
    /// 课程表的课程节次定义列表
    /// </summary>
    private List<PeriodDefinition> _periodDefinitions = [];

    /// <summary>
    /// 课程表的课程列表
    /// </summary>
    private readonly List<Course> _courses = [];

    /// <summary>
    /// 私有构造函数, 防止外部直接实例化, 只能通过工厂方法创建
    /// </summary>
    private Timetable() { }

    /// <summary>
    /// 创建一个新的课程表实例
    /// </summary>
    /// <param name="name">课程表名称</param>
    /// <param name="firstDay">课程表的第一周的任意一天日期, 内部会自动转换为该周的星期一日期</param>
    /// <param name="totalWeeks">课程表总周数</param>
    /// <returns>操作结果, 成功时包含新创建的课程表实例</returns>
    /// <exception cref="ArgumentOutOfRangeException">参数不满足约束时抛出</exception>
    /// <exception cref="ArgumentException">参数 为 <see langword="null"/> 或空白字符时抛出</exception>
    public static Result Create(string name, DateOnly firstDay, int totalWeeks)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalWeeks, nameof(totalWeeks));

        if (totalWeeks > MaxTotalWeeks)
        {
            return Result.Failure(
                ErrorCode.TotalWeeksExceedLimit,
                $"总周数 {totalWeeks} 超过允许的最大值 {MaxTotalWeeks}"
            );
        }

        var timetable = new Timetable
        {
            Id = Guid.NewGuid(),
            Name = name,
            FirstMonday = firstDay,
            TotalWeeks = totalWeeks
        };
        return Result.Success(timetable);
    }

    /// <summary>
    /// 确保课程节次定义列表按序号升序排列, 由 EF Core 在物化后调用
    /// </summary>
    public void EnsurePeriodDefinitionsSorted()
    {
        _periodDefinitions.Sort((a, b) => a.Ordinal.CompareTo(b.Ordinal));
    }

    /// <summary>
    /// 更改课程表的总周数
    /// </summary>
    /// <param name="totalWeeks">课程表总周数</param>
    /// <returns>操作结果</returns>
    /// <exception cref="ArgumentOutOfRangeException">参数不满足约束时抛出</exception>
    public Result ChangeTotalWeeks(int totalWeeks)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalWeeks, nameof(totalWeeks));

        if (totalWeeks > MaxTotalWeeks)
        {
            return Result.Failure(
                ErrorCode.TotalWeeksExceedLimit,
                $"要更改的总周数 {totalWeeks} 超过允许的最大值 {MaxTotalWeeks}"
            );
        }

        if (totalWeeks < TotalWeeks)
        {
            foreach (var course in _courses)
            {
                foreach (var fragment in course._fragments)
                {
                    var lastWeek = fragment._weeks[^1].End;
                    if (lastWeek > totalWeeks)
                    {
                        return Result.Failure(
                            ErrorCode.WeekOccupied,
                            $"课程 {course.Name} 上课到第 {lastWeek} 周, 无法将总周数减小到 {totalWeeks} 周"
                        );
                    }
                }
            }
        }

        TotalWeeks = totalWeeks;
        return Result.Success();
    }

    /// <inheritdoc/>
    public bool Equals(Timetable? other)
    {
        return Id == other?.Id;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Timetable other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
