using System;

namespace ClassSchedule.Domain.Entities;

/// <summary>
/// 表示一个课程节次定义
/// </summary>
public sealed class PeriodDefinition : IEquatable<PeriodDefinition>
{
    /// <summary>
    /// 序数
    /// </summary>
    public int Ordinal { get; internal init; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public TimeOnly StartTime { get; internal set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public TimeOnly EndTime { get; internal set; }

    /// <summary>
    /// 持续时间
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// 内部构造函数, 防止外部实例化
    /// </summary>
    internal PeriodDefinition() { }

    /// <inheritdoc/>
    public bool Equals(PeriodDefinition? other)
    {
        return Ordinal == other?.Ordinal;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is PeriodDefinition other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Ordinal.GetHashCode();
    }
}
