using System.ComponentModel;

namespace ClassSchedule.Domain.Models;

/// <summary>
/// 错误码枚举, 用于表示不同类型的错误
/// </summary>
public enum ErrorCode
{
    /// <summary>
    /// 未知错误
    /// </summary>
    [Description("未知错误")]
    Unknown,

    /// <summary>
    /// 课程表未找到
    /// </summary>
    [Description("课程表未找到")]
    TimetableNotFound,

    /// <summary>
    /// 总周数超过限制
    /// </summary>
    [Description("总周数超过限制")]
    TotalWeeksExceedLimit,

    /// <summary>
    /// 周次不存在
    /// </summary>
    [Description("周次不存在")]
    WeekNotFound,

    /// <summary>
    /// 周次重叠
    /// </summary>
    [Description("周次重叠")]
    WeekOverlap,

    /// <summary>
    /// 周次被占用无法减小总周数
    /// </summary>
    [Description("周次被占用无法减小总周数")]
    WeekOccupied,

    /// <summary>
    /// 课程未找到
    /// </summary>
    [Description("课程未找到")]
    CourseNotFound,

    /// <summary>
    /// 课程片段未找到
    /// </summary>
    [Description("课程片段未找到")]
    FragmentNotFound,

    /// <summary>
    /// 课程片段时间重叠
    /// </summary>
    [Description("课程片段时间重叠")]
    FragmentOverlap,

    /// <summary>
    /// 课程节次数量超过限制
    /// </summary>
    [Description("课程节次数量超过限制")]
    PeriodCountExceedLimit,

    /// <summary>
    /// 课程节次时间重叠
    /// </summary>
    [Description("课程节次时间重叠")]
    PeriodOverlap,

    /// <summary>
    /// 课程节次未找到
    /// </summary>
    [Description("课程节次未找到")]
    PeriodNotFound,

    /// <summary>
    /// 课程节次被占用无法移除
    /// </summary>
    [Description("课程节次被占用无法移除")]
    PeriodOccupied
}
