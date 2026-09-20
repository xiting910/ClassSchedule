using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ClassSchedule.UI.Shared.Models;
using ClassSchedule.UI.Shared.ViewModels;

namespace ClassSchedule.UI.Shared.Views;

/// <summary>
/// 课表列表视图, 用于切换当前课表, 以及重命名或删除课表
/// </summary>
public sealed partial class TimetableListView : UserControl
{
    /// <summary>
    /// 构造函数, 初始化组件
    /// </summary>
    public TimetableListView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 点击名称区时把该课表设为当前课表
    /// </summary>
    /// <param name="sender">名称区</param>
    /// <param name="e">点击事件参数</param>
    private void OnSelectTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control { DataContext: TimetableListItem item }
        && DataContext is TimetableListViewModel viewModel)
        {
            viewModel.Select(item);
        }
    }

    /// <summary>
    /// 点击重命名按钮时进入行内重命名状态
    /// </summary>
    /// <param name="sender">重命名按钮</param>
    /// <param name="e">路由事件参数</param>
    private void OnRenameClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: TimetableListItem item })
        {
            item.EditingName = item.Name;
            item.IsRenaming = true;
        }
    }

    /// <summary>
    /// 点击确定按钮时提交行内重命名
    /// </summary>
    /// <param name="sender">确定按钮</param>
    /// <param name="e">路由事件参数</param>
    private async void OnCommitRenameClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: TimetableListItem item }
        && DataContext is TimetableListViewModel viewModel)
        {
            await viewModel.CommitRenameAsync(item);
        }
    }

    /// <summary>
    /// 点击取消按钮时放弃行内重命名
    /// </summary>
    /// <param name="sender">取消按钮</param>
    /// <param name="e">路由事件参数</param>
    private void OnCancelRenameClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: TimetableListItem item })
        {
            item.IsRenaming = false;
        }
    }

    /// <summary>
    /// 点击删除按钮时请求二次确认
    /// </summary>
    /// <param name="sender">删除按钮</param>
    /// <param name="e">路由事件参数</param>
    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: TimetableListItem item }
        && DataContext is TimetableListViewModel viewModel)
        {
            viewModel.RequestDelete(item);
        }
    }
}
