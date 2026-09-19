using Avalonia.Data.Converters;
using System;

namespace ClassSchedule.UI.Shared;

/// <summary>
/// <see cref="TimeOnly"/> 与 <see cref="TimeSpan"/> 的双向转换器
/// </summary>
public sealed class TimeOnlyConverter : FuncValueConverter<TimeOnly?, TimeSpan?>
{
    /// <summary>
    /// 构造函数, 装配两个方向的换算
    /// </summary>
    public TimeOnlyConverter() : base(time => time?.ToTimeSpan(), ToTimeOnly) { }

    /// <summary>
    /// 把控件给出的时长换算回时刻
    /// </summary>
    /// <param name="span">控件给出的时长</param>
    /// <returns>对应的时刻</returns>
    private static TimeOnly? ToTimeOnly(TimeSpan? span)
    {
        return span is { } value ? TimeOnly.FromTimeSpan(value) : null;
    }
}
