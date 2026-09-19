using Avalonia.Controls;
using Avalonia.Interactivity;
using ClassSchedule.UI.Shared.Models;
using ClassSchedule.UI.Shared.ViewModels;

namespace ClassSchedule.UI.Shared.Views;

/// <summary>
/// 新建课表视图, 预填 12 节时间模板, 允许逐行改时间与增删节次
/// </summary>
public sealed partial class CreateTimetableView : UserControl
{
    /// <summary>
    /// 构造函数, 初始化组件
    /// </summary>
    public CreateTimetableView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 点击删除按钮时移除该节
    /// </summary>
    /// <param name="sender">删除按钮</param>
    /// <param name="e">路由事件参数</param>
    private void OnRemovePeriodClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: PeriodInputRow row }
        && DataContext is CreateTimetableViewModel viewModel)
        {
            viewModel.RemovePeriod(row);
        }
    }
}
