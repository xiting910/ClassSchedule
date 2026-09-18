using Avalonia.Controls;

namespace ClassSchedule.UI.Shared.Views;

/// <summary>
/// 确认对话框视图, 只承载内容, 卡片外观由 ShellView 的卡片容器负责
/// </summary>
public sealed partial class ConfirmView : UserControl
{
    /// <summary>
    /// 构造函数, 初始化组件
    /// </summary>
    public ConfirmView()
    {
        InitializeComponent();
    }
}
