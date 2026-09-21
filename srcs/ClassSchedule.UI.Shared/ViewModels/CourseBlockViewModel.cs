using Avalonia.Media;
using Avalonia.Media.Immutable;
using ClassSchedule.Domain.Models;
using System;
using System.Collections.Generic;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 周视图网格里一个课程块, 一个块对应一个课程片段
/// </summary>
/// <param name="CourseId">课程片段所在课程的唯一标识符</param>
/// <param name="FragmentId">课程片段的唯一标识符</param>
/// <param name="Weekday">上课日期</param>
/// <param name="Period">上课节次</param>
/// <param name="Weeks">上课周次</param>
/// <param name="Name">课程名称</param>
/// <param name="Color">课程颜色</param>
/// <param name="Location">上课地点</param>
/// <param name="Teacher">上课教师</param>
public sealed record CourseBlockViewModel(
    Guid CourseId,
    Guid FragmentId,
    Weekday Weekday,
    OrdinalRange Period,
    IReadOnlyList<OrdinalRange> Weeks,
    string Name,
    string Color,
    string? Location,
    string? Teacher)
{
    /// <summary>
    /// 课程块所在的行
    /// </summary>
    public int Row => Period.Start - 1;

    /// <summary>
    /// 课程块所在的列
    /// </summary>
    public int Column => (int)Weekday - 1;

    /// <summary>
    /// 课程块纵向跨越的行数
    /// </summary>
    public int RowSpan => Period.End - Period.Start + 1;

    /// <summary>
    /// 课程块的背景画刷
    /// </summary>
    public IBrush? Background => Avalonia.Media.Color.TryParse(Color, out var color)
        ? new ImmutableSolidColorBrush(color)
        : null;
}
