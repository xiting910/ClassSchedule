using ClassSchedule.Infrastructure.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ClassSchedule.UI.Shared.Models;

/// <summary>
/// 课表列表项, 承载一行的显示内容与行内重命名状态
/// </summary>
/// <param name="summary">课表摘要</param>
public sealed partial class TimetableListItem(TimetableSummary summary) : ObservableObject
{
    /// <summary>
    /// 课表 Id
    /// </summary>
    public Guid Id { get; } = summary.Id;

    /// <summary>
    /// 学期信息
    /// </summary>
    public string RangeText { get; } =
        $"{summary.FirstMonday:yyyy-MM-dd} ~" +
        $" {summary.FirstMonday.AddDays((summary.TotalWeeks * 7) - 1):yyyy-MM-dd} " +
        $"· 共 {summary.TotalWeeks} 周";

    /// <summary>
    /// 课表名称
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = summary.Name;

    /// <summary>
    /// 是否为当前正在使用的课表
    /// </summary>
    [ObservableProperty]
    public partial bool IsCurrent { get; set; }

    /// <summary>
    /// 是否处于行内重命名状态
    /// </summary>
    [ObservableProperty]
    public partial bool IsRenaming { get; set; }

    /// <summary>
    /// 行内重命名时的名称草稿
    /// </summary>
    [ObservableProperty]
    public partial string EditingName { get; set; } = summary.Name;
}
