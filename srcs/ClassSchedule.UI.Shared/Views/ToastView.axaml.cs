using Avalonia.Controls;
using Avalonia.Input;
using ClassSchedule.UI.Shared.Models;
using ClassSchedule.UI.Shared.ViewModels;

namespace ClassSchedule.UI.Shared.Views;

/// <summary>
/// 提示条目视图, 用于显示短暂提示
/// </summary>
public sealed partial class ToastView : UserControl
{
    /// <summary>
    /// 构造函数, 初始化组件
    /// </summary>
    public ToastView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 点击提示条目时立即关闭该条目并执行其点击回调
    /// </summary>
    /// <param name="sender">提示条目</param>
    /// <param name="e">点击事件参数</param>
    private void OnToastTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border { DataContext: Toast item } && DataContext is ToastViewModel viewModel)
        {
            viewModel.InvokeClick(item);
        }
    }
}
