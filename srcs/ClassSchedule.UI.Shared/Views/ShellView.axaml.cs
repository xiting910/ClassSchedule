using Avalonia.Controls;
using Avalonia.Interactivity;
using ClassSchedule.UI.Shared.ViewModels;

namespace ClassSchedule.UI.Shared.Views;

/// <summary>
/// 壳视图, 承载整个应用的页面栈与全局提示
/// </summary>
public sealed partial class ShellView : UserControl
{
    /// <summary>
    /// 当前挂载的顶级控件, 用于在卸载时摘掉返回键钩子
    /// </summary>
    private TopLevel? _topLevel;

    /// <summary>
    /// 构造函数, 初始化组件并挂上加载与卸载钩子
    /// </summary>
    public ShellView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 挂载到可视树时接住物理返回键
    /// </summary>
    /// <param name="sender">壳视图</param>
    /// <param name="e">路由事件参数</param>
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _topLevel = TopLevel.GetTopLevel(this);
        _topLevel?.BackRequested += OnBackRequested;
    }

    /// <summary>
    /// 离开可视树时摘掉返回键钩子
    /// </summary>
    /// <param name="sender">壳视图</param>
    /// <param name="e">路由事件参数</param>
    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        _topLevel?.BackRequested -= OnBackRequested;
        _topLevel = null;
    }

    /// <summary>
    /// 处理物理返回键, 由壳视图模型决定是否消费
    /// </summary>
    /// <param name="sender">顶级控件</param>
    /// <param name="e">路由事件参数</param>
    private void OnBackRequested(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel viewModel && viewModel.TryGoBack())
        {
            e.Handled = true;
        }
    }
}
