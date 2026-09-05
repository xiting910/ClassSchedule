using System;

namespace ClassSchedule.Domain.Models;

/// <summary>
/// 表示一个闭区间周段, 两端都包含
/// </summary>
public readonly record struct WeekRange
{
    /// <summary>
    /// 起始周号
    /// </summary>
    public int StartWeek { get; }

    /// <summary>
    /// 结束周号
    /// </summary>
    public int EndWeek { get; }

    /// <summary>
    /// 构造函数, 使用起始周号和结束周号初始化周段
    /// </summary>
    /// <param name="startWeek">起始周号, 必须为正整数</param>
    /// <param name="endWeek">结束周号, 必须不小于起始周号</param>
    /// <exception cref="ArgumentOutOfRangeException">参数不满足约束时抛出</exception>
    public WeekRange(int startWeek, int endWeek)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(startWeek, nameof(startWeek));
        ArgumentOutOfRangeException.ThrowIfLessThan(endWeek, startWeek, nameof(endWeek));

        StartWeek = startWeek;
        EndWeek = endWeek;
    }

    /// <summary>
    /// 判断指定周号是否在本周段内 (含两端)
    /// </summary>
    /// <param name="week">要判断的周号</param>
    /// <returns><see langword="true"/> 如果在本周段内, 否则为 <see langword="false"/></returns>
    public bool ContainsWeek(int week)
    {
        return StartWeek <= week && week <= EndWeek;
    }

    /// <summary>
    /// 判断本周段是否与另一周段共享至少一个周号
    /// </summary>
    /// <param name="other">另一周段</param>
    /// <returns><see langword="true"/> 如果两段存在共同周号, 否则为 <see langword="false"/></returns>
    public bool Intersects(WeekRange other)
    {
        return StartWeek <= other.EndWeek && other.StartWeek <= EndWeek;
    }
}
