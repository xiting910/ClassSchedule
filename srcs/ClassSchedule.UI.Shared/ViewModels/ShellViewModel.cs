using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 壳视图模型, 承载整个应用的视图模型, 负责管理全局状态与导航
/// </summary>
/// <param name="navigationStack">导航栈</param>
/// <param name="overlayHost">浮层宿主视图模型</param>
/// <param name="toast">全局提示视图模型</param>
/// <param name="uiOptions">UI 配置</param>
public sealed partial class ShellViewModel(
    NavigationStack navigationStack,
    OverlayHostViewModel overlayHost,
    ToastViewModel toast,
    UIOptions uiOptions
) : ObservableObject
{
    /// <summary>
    /// 导航栈
    /// </summary>
    public NavigationStack NavigationStack { get; } = navigationStack;

    /// <summary>
    /// 浮层宿主视图模型, 用于显示浮层内容
    /// </summary>
    public OverlayHostViewModel OverlayHost { get; } = overlayHost;

    /// <summary>
    /// 全局提示视图模型, 供壳与各页面显示提示
    /// </summary>
    public ToastViewModel Toast { get; } = toast;

    /// <summary>
    /// 当前选中的底部 Tab 索引
    /// </summary>
    [ObservableProperty]
    public partial int SelectedTabIndex { get; set; }

    /// <summary>
    /// 按首页决策打开启动页
    /// </summary>
    public async void OpenHomePage()
    {
        if (uiOptions.CurrentTimetableId is not { } currentTimetableId)
        {
            await NavigationStack.PushAsync<TimetableListViewModel>();
            return;
        }

        // TODO: 使用 currentTimetableId 显示详细课表页
    }

    /// <summary>
    /// 处理一次返回请求
    /// </summary>
    /// <returns><see langword="true"/> 表示已消费, 否则为 <see langword="false"/></returns>
    public bool TryGoBack()
    {
        if (OverlayHost.HasOverlay)
        {
            OverlayHost.Close();
            return true;
        }

        if (NavigationStack.TryPop())
        {
            return true;
        }

        if (SelectedTabIndex != 0)
        {
            SelectedTabIndex = 0;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 打开课表列表页
    /// </summary>
    [RelayCommand]
    private Task OpenTimetableListAsync()
    {
        return NavigationStack.PushAsync<TimetableListViewModel>();
    }
}
