namespace ClassSchedule.UI.Shared.Models;

/// <summary>
/// 周视图的周次选择项, 承载一个周次的显示内容
/// </summary>
/// <param name="Week">周次</param>
/// <param name="IsCurrent">是否为当前周</param>
/// <param name="IsDisplayed">是否为当前显示的周次</param>
public sealed record WeekNumberItem(int Week, bool IsCurrent, bool IsDisplayed);
