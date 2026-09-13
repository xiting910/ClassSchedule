using System;
using System.Collections.Generic;

namespace ClassSchedule.Domain.Entities;

/// <summary>
/// 表示一个课程
/// </summary>
public sealed class Course : IEquatable<Course>
{
    /// <summary>
    /// 唯一标识符
    /// </summary>
    public Guid Id { get; internal init; }

    /// <summary>
    /// 课程名称
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
    /// 课程颜色
    /// </summary>
    /// <exception cref="ArgumentException">参数 为 <see langword="null"/> 或空白字符时抛出</exception>
    public required string Color
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(Color));
            field = value;
        }
    }

    /// <summary>
    /// 课程备注
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// 课程片段只读列表
    /// </summary>
    public IReadOnlyList<Fragment> Fragments => _fragments;

    /// <summary>
    /// 课程片段列表
    /// </summary>
    internal readonly List<Fragment> _fragments = [];

    /// <summary>
    /// 内部构造函数, 防止外部实例化
    /// </summary>
    internal Course() { }

    /// <inheritdoc/>
    public bool Equals(Course? other)
    {
        return Id == other?.Id;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Course other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
