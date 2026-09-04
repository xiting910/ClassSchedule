using Avalonia.Controls;

namespace ClassSchedule.UI.Shared.Views;

/// <summary>
/// 壳视图, 承载整个应用的视图模型, 负责管理全局状态与导航
/// </summary>
public sealed partial class ShellView : UserControl
{
    /// <summary>
    /// 构造函数, 初始化组件
    /// </summary>
    public ShellView()
    {
        InitializeComponent();
    }
}
