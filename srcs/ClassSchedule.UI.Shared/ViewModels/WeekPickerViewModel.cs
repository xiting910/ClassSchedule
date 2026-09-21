using ClassSchedule.UI.Shared.Models;
using System;
using System.Collections.Generic;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 周次选择浮层视图模型, 列出所有周次供用户切换当前显示的周
/// </summary>
/// <param name="weeks">周次项列表</param>
/// <param name="switchWeek">选中某一周之后要执行的动作</param>
/// <param name="onClosing">关闭浮层的回调</param>
public sealed class WeekPickerViewModel(
    IEnumerable<WeekNumberItem> weeks,
    Action<int> switchWeek,
    Action onClosing
) : OverlayViewModel
{
    /// <summary>
    /// 周次项列表
    /// </summary>
    public IReadOnlyList<WeekNumberItem> Weeks { get; } = [.. weeks];

    /// <summary>
    /// 切换到指定周次并关闭浮层
    /// </summary>
    /// <param name="item">被选中的周次项</param>
    public void Select(WeekNumberItem item)
    {
        onClosing();
        switchWeek(item.Week);
    }
}
