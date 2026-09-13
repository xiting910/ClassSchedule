using ClassSchedule.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ClassSchedule.Domain.Entities;

// Timetable 的分部类, 用于定义课程片段相关的操作
#pragma warning disable IDE0040 // 分部类不需要可访问性修饰符
partial class Timetable
#pragma warning restore IDE0040 // 分部类不需要可访问性修饰符
{
    /// <summary>
    /// 创建并添加一个新的课程片段到指定的课程中
    /// </summary>
    /// <param name="courseId">课程片段所在课程的唯一标识符</param>
    /// <param name="weekday">上课日期</param>
    /// <param name="period">上课节次</param>
    /// <param name="weeks">上课周次列表</param>
    /// <param name="teacher">上课教师</param>
    /// <param name="location">上课地点</param>
    /// <returns>操作结果, 成功时包含新创建的课程片段实例</returns>
    /// <exception cref="ArgumentNullException">参数 为 <see langword="null"/> 时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">参数不满足约束时抛出</exception>
    public Result AddFragmentToCourse(
        Guid courseId,
        Weekday weekday,
        OrdinalRange period,
        IEnumerable<OrdinalRange> weeks,
        string? teacher = null,
        string? location = null)
    {
        ArgumentNullException.ThrowIfNull(weeks, nameof(weeks));

        var weekList = weeks.ToList();
        ArgumentOutOfRangeException.ThrowIfZero(weekList.Count, nameof(weeks));

        var index = _courses.FindIndex(c => c.Id == courseId);
        if (index == -1)
        {
            return Result.Failure(ErrorCode.CourseNotFound, $"课程 {courseId} 不存在");
        }

        if (period.End > _periodDefinitions.Count)
        {
            return Result.Failure(
                ErrorCode.PeriodNotFound,
                $"课程节次 {period.Start} - {period.End} 不存在, 课程表中的最大节次为 {_periodDefinitions.Count}"
            );
        }

        if (!OrdinalRange.TryNormalize(weekList, out var overlap1, out var overlap2))
        {
            return Result.Failure(
                ErrorCode.WeekOverlap,
                $"课程周次 {overlap1.Start} - {overlap1.End} 与 {overlap2.Start} - {overlap2.End} 存在重叠"
            );
        }

        if (weekList[^1].End > TotalWeeks)
        {
            return Result.Failure(
                ErrorCode.WeekNotFound,
                $"课程周次 {weekList[^1].Start} - {weekList[^1].End} 不存在, 课程表总周数为 {TotalWeeks}"
            );
        }

        var course = _courses[index];
        var fragments = course._fragments;
        if (IsFragmentOverlap(fragments, weekday, period, weekList))
        {
            return Result.Failure(
                ErrorCode.FragmentOverlap,
                $"要添加的课程片段会导致课程 {course.Name} 的不同课程片段的上课时间重叠"
            );
        }

        var fragment = new Fragment
        {
            Id = Guid.NewGuid(),
            Teacher = teacher,
            Location = location,
            Weekday = weekday,
            Period = period,
            _weeks = weekList
        };
        fragments.Add(fragment);
        return Result.Success(fragment);
    }

    /// <summary>
    /// 从指定的课程中移除一个课程片段
    /// </summary>
    /// <param name="courseId">课程片段所在课程的唯一标识符</param>
    /// <param name="fragmentId">课程片段的唯一标识符</param>
    /// <returns>操作结果</returns>
    public Result RemoveFragmentFromCourse(Guid courseId, Guid fragmentId)
    {
        if (!FindFragment(courseId, fragmentId, out var result, out var course, out var fragmentIndex))
        {
            return result;
        }

        course._fragments.RemoveAt(fragmentIndex);
        return Result.Success();
    }

    /// <summary>
    /// 更改指定课程片段的上课日期
    /// </summary>
    /// <param name="courseId">课程片段所在课程的唯一标识符</param>
    /// <param name="fragmentId">课程片段的唯一标识符</param>
    /// <param name="weekday">新的上课日期</param>
    /// <returns>操作结果</returns>
    public Result ChangeFragmentWeekday(Guid courseId, Guid fragmentId, Weekday weekday)
    {
        if (!FindFragment(courseId, fragmentId, out var result, out var course, out var index))
        {
            return result;
        }

        var fragments = course._fragments;
        var isOverlap = IsFragmentOverlap(
            fragments.Where(f => f.Id != fragmentId),
            weekday,
            fragments[index].Period,
            fragments[index]._weeks
        );
        if (isOverlap)
        {
            return Result.Failure(
                ErrorCode.FragmentOverlap,
                $"要更改的上课日期会导致课程 {course.Name} 的不同课程片段的上课时间重叠"
            );
        }

        fragments[index].Weekday = weekday;
        return Result.Success();
    }

    /// <summary>
    /// 更改指定课程片段的上课节次
    /// </summary>
    /// <param name="courseId">课程片段所在课程的唯一标识符</param>
    /// <param name="fragmentId">课程片段的唯一标识符</param>
    /// <param name="period">新的上课节次</param>
    /// <returns>操作结果</returns>
    public Result ChangeFragmentPeriod(Guid courseId, Guid fragmentId, OrdinalRange period)
    {
        if (period.End > _periodDefinitions.Count)
        {
            return Result.Failure(
                ErrorCode.PeriodNotFound,
                $"课程节次 {period.Start} - {period.End} 不存在, 课程表中的最大节次为 {_periodDefinitions.Count}"
            );
        }

        if (!FindFragment(courseId, fragmentId, out var result, out var course, out var index))
        {
            return result;
        }

        var fragments = course._fragments;
        var isOverlap = IsFragmentOverlap(
            fragments.Where(f => f.Id != fragmentId),
            fragments[index].Weekday,
            period,
            fragments[index]._weeks
        );
        if (isOverlap)
        {
            return Result.Failure(
                ErrorCode.FragmentOverlap,
                $"要更改的课程节次会导致课程 {course.Name} 的不同课程片段的上课时间重叠"
            );
        }

        fragments[index].Period = period;
        return Result.Success();
    }

    /// <summary>
    /// 更改指定课程片段的上课周次
    /// </summary>
    /// <param name="courseId">课程片段所在课程的唯一标识符</param>
    /// <param name="fragmentId">课程片段的唯一标识符</param>
    /// <param name="weeks">新的上课周次</param>
    /// <returns>操作结果</returns>
    /// <exception cref="ArgumentNullException">参数 为 <see langword="null"/> 时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">参数不满足约束时抛出</exception>
    public Result ChangeFragmentWeeks(Guid courseId, Guid fragmentId, IEnumerable<OrdinalRange> weeks)
    {
        ArgumentNullException.ThrowIfNull(weeks, nameof(weeks));

        var weekList = weeks.ToList();
        ArgumentOutOfRangeException.ThrowIfZero(weekList.Count, nameof(weeks));

        if (!OrdinalRange.TryNormalize(weekList, out var overlap1, out var overlap2))
        {
            return Result.Failure(
                ErrorCode.WeekOverlap,
                $"课程周次 {overlap1.Start} - {overlap1.End} 与 {overlap2.Start} - {overlap2.End} 存在重叠"
            );
        }

        if (weekList[^1].End > TotalWeeks)
        {
            return Result.Failure(
                ErrorCode.WeekNotFound,
                $"课程周次 {weekList[^1].Start} - {weekList[^1].End} 不存在, 课程表总周数为 {TotalWeeks}"
            );
        }

        if (!FindFragment(courseId, fragmentId, out var result, out var course, out var index))
        {
            return result;
        }

        var fragments = course._fragments;
        var isOverlap = IsFragmentOverlap(
            fragments.Where(f => f.Id != fragmentId),
            fragments[index].Weekday,
            fragments[index].Period,
            weekList
        );
        if (isOverlap)
        {
            return Result.Failure(
                ErrorCode.FragmentOverlap,
                $"要更改的课程周次会导致课程 {course.Name} 的不同课程片段的上课时间重叠"
            );
        }

        fragments[index]._weeks = weekList;
        return Result.Success();
    }

    /// <summary>
    /// 查找指定的课程片段
    /// </summary>
    /// <param name="courseId">课程的唯一标识符</param>
    /// <param name="fragmentId">课程片段的唯一标识符</param>
    /// <param name="result">失败时返回操作结果</param>
    /// <param name="course">课程</param>
    /// <param name="fragmentIndex">课程片段在课程中的索引</param>
    /// <returns><see langword="true"/> 表示找到了指定的课程片段, <see langword="false"/> 表示未找到</returns>
    private bool FindFragment(
        Guid courseId,
        Guid fragmentId,
        [NotNullWhen(false)] out Result? result,
        [NotNullWhen(true)] out Course? course,
        out int fragmentIndex)
    {
        var courseIndex = _courses.FindIndex(c => c.Id == courseId);
        if (courseIndex == -1)
        {
            result = Result.Failure(ErrorCode.CourseNotFound, $"课程 {courseId} 不存在");
            course = null;
            fragmentIndex = -1;
            return false;
        }

        course = _courses[courseIndex];
        fragmentIndex = course._fragments.FindIndex(f => f.Id == fragmentId);
        if (fragmentIndex == -1)
        {
            result = Result.Failure(ErrorCode.FragmentNotFound, $"课程片段 {fragmentId} 不存在");
            return false;
        }

        result = null;
        return true;
    }

    /// <summary>
    /// 判断指定的课程片段值是否与指定的课程片段列表存在时间重叠
    /// </summary>
    /// <param name="fragments">课程片段列表</param>
    /// <param name="weekday">上课日期</param>
    /// <param name="period">上课节次</param>
    /// <param name="weeks">上课周次列表</param>
    /// <returns><see langword="true"/> 表示存在时间重叠, <see langword="false"/> 表示不存在时间重叠</returns>
    private static bool IsFragmentOverlap(
        IEnumerable<Fragment> fragments,
        Weekday weekday,
        OrdinalRange period,
        IEnumerable<OrdinalRange> weeks)
    {
        List<OrdinalRange> mergedWeeks = [];
        foreach (var fragment in fragments)
        {
            if (fragment.Weekday == weekday && fragment.Period.Intersects(period))
            {
                mergedWeeks.Clear();
                mergedWeeks.AddRange(weeks);
                mergedWeeks.AddRange(fragment._weeks);
                if (!OrdinalRange.TryNormalize(mergedWeeks, out var _, out var _))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
