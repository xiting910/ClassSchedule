using ClassSchedule.Domain.Models;
using System;

namespace ClassSchedule.Domain.Entities;

// Timetable 的分部类, 用于定义课程相关的操作
#pragma warning disable IDE0040 // 分部类不需要可访问性修饰符
partial class Timetable
#pragma warning restore IDE0040 // 分部类不需要可访问性修饰符
{
    /// <summary>
    /// 创建并添加一个新的课程到课程表中
    /// </summary>
    /// <param name="name">课程名称</param>
    /// <param name="Color">课程颜色</param>
    /// <param name="note">课程备注</param>
    /// <returns>操作结果, 成功时包含新创建的课程实例</returns>
    /// <exception cref="ArgumentException">参数 为 <see langword="null"/> 或空白字符时抛出</exception>
    public Result AddCourse(string name, string Color, string? note = null)
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Name = name,
            Color = Color,
            Note = note
        };

        _courses.Add(course);
        return Result.Success(course);
    }

    /// <summary>
    /// 从课程表中移除指定的课程
    /// </summary>
    /// <param name="courseId">课程的唯一标识符</param>
    /// <returns>操作结果</returns>
    public Result RemoveCourse(Guid courseId)
    {
        var index = _courses.FindIndex(c => c.Id == courseId);
        if (index == -1)
        {
            return Result.Failure(ErrorCode.CourseNotFound, $"课程 {courseId} 不存在");
        }

        _courses.RemoveAt(index);
        return Result.Success();
    }
}
