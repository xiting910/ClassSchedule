using ClassSchedule.Domain.Models;
using System;
using System.Collections.Generic;

namespace ClassSchedule.Domain.Entities;

/// <summary>
/// 表示一个课程片段
/// </summary>
public sealed class Fragment : IEquatable<Fragment>
{
    /// <summary>
    /// 唯一标识符
    /// </summary>
    public Guid Id { get; internal init; }

    /// <summary>
    /// 上课教师
    /// </summary>
    public string? Teacher { get; set; }

    /// <summary>
    /// 上课地点
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 上课日期
    /// </summary>
    public Weekday Weekday { get; internal set; }

    /// <summary>
    /// 上课节次
    /// </summary>
    public OrdinalRange Period { get; internal set; }

    /// <summary>
    /// 上课周次只读列表
    /// </summary>
    public IReadOnlyList<OrdinalRange> Weeks => _weeks;

    /// <summary>
    /// 上课周次列表, 任何时刻都应该已经经过归一化处理
    /// </summary>
    internal List<OrdinalRange> _weeks = [];

    /// <summary>
    /// 内部构造函数, 防止外部实例化
    /// </summary>
    internal Fragment() { }

    /// <summary>
    /// 确保课程片段的周次列表已经排序, 由 EF Core 在物化时调用
    /// </summary>
    public void EnsureWeeksSorted()
    {
        _weeks.Sort((a, b) => a.Start.CompareTo(b.Start));
    }

    /// <inheritdoc/>
    public bool Equals(Fragment? other)
    {
        return Id == other?.Id;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Fragment other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
