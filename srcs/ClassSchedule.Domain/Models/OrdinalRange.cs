using System;
using System.Collections.Generic;

namespace ClassSchedule.Domain.Models;

/// <summary>
/// 表示一个序数闭区间, 两端都包含
/// </summary>
public readonly record struct OrdinalRange
{
    /// <summary>
    /// 起始序数
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// 结束序数
    /// </summary>
    public int End { get; }

    /// <summary>
    /// 构造函数, 使用起始序数与结束序数初始化闭区间
    /// </summary>
    /// <param name="start">起始序数, 必须为正整数</param>
    /// <param name="end">结束序数, 必须不小于起始序数</param>
    /// <exception cref="ArgumentOutOfRangeException">参数不满足约束时抛出</exception>
    public OrdinalRange(int start, int end)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(start, nameof(start));
        ArgumentOutOfRangeException.ThrowIfLessThan(end, start, nameof(end));

        Start = start;
        End = end;
    }

    /// <summary>
    /// 判断指定序数是否在本区间内 (含两端)
    /// </summary>
    /// <param name="value">要判断的序数</param>
    /// <returns><see langword="true"/> 如果在本区间内, 否则为 <see langword="false"/></returns>
    public bool Contains(int value)
    {
        return Start <= value && value <= End;
    }

    /// <summary>
    /// 判断本区间是否与另一区间共享至少一个序数
    /// </summary>
    /// <param name="other">另一区间</param>
    /// <returns><see langword="true"/> 如果两区间存在共同序数, 否则为 <see langword="false"/></returns>
    public bool Intersects(OrdinalRange other)
    {
        return Start <= other.End && other.Start <= End;
    }

    /// <summary>
    /// 尝试将指定的区间列表归一化: 就地修改传入的列表, 将列表排序后合并相邻区间
    /// </summary>
    /// <remarks><para>
    /// 在修改列表的过程中, 如果发现存在重叠的区间, 则会立刻返回失败, 并通过输出参数返回重叠的两个区间
    /// </para><para>
    /// 因此归一化失败时列表可能停留在部分合并的中间态
    /// </para></remarks>
    /// <param name="ranges">要归一化的区间列表, 会被就地修改</param>
    /// <param name="r1">存在重叠的区间时, 重叠的第一个区间</param>
    /// <param name="r2">存在重叠的区间时, 重叠的第二个区间</param>
    /// <returns><see langword="true"/> 如果归一化成功, 否则为 <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException">参数为 <see langword="null"/> 时抛出</exception>
    public static bool TryNormalize(List<OrdinalRange> ranges, out OrdinalRange r1, out OrdinalRange r2)
    {
        ArgumentNullException.ThrowIfNull(ranges, nameof(ranges));

        if (ranges.Count < 2)
        {
            r1 = r2 = default;
            return true;
        }

        ranges.Sort(static (a, b) => a.Start.CompareTo(b.Start));
        for (var i = ranges.Count - 1; i > 0; i--)
        {
            r1 = ranges[i - 1];
            r2 = ranges[i];
            if (r1.End >= r2.Start)
            {
                return false;
            }

            if (r1.End + 1 == r2.Start)
            {
                ranges.RemoveAt(i);
                ranges[i - 1] = new(r1.Start, r2.End);
            }
        }

        r1 = r2 = default;
        return true;
    }
}
