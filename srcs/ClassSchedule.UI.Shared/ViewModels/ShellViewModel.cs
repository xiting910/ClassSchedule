using ClassSchedule.Domain.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 壳视图模型, 承载整个应用的视图模型, 负责管理全局状态与导航
/// </summary>
/// <param name="confirmLogger">确认对话框视图模型的日志记录器</param>
/// <param name="shellLogger">壳视图模型的日志记录器</param>
/// <param name="scopeFactory">服务范围工厂</param>
/// <param name="toast">提示视图模型</param>
/// <param name="uiOptions">UI 配置</param>
public sealed partial class ShellViewModel(
    ILogger<ConfirmViewModel> confirmLogger,
    ILogger<ShellViewModel> shellLogger,
    IServiceScopeFactory scopeFactory,
    ToastViewModel toast,
    UIOptions uiOptions
) : ObservableObject
{
    /// <summary>
    /// 确认对话框关闭时的纵向偏移
    /// </summary>
    private const double ConfirmHiddenOffsetY = 16.0;

    /// <summary>
    /// 导航栈里的一页: 页面级服务范围与它的视图模型
    /// </summary>
    /// <param name="Scope">页面级服务范围, 出栈时释放</param>
    /// <param name="ViewModel">页面视图模型, 由容器在该范围内创建</param>
    private sealed record PageEntry(IServiceScope Scope, IPageViewModel ViewModel);

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<ShellViewModel> _logger = shellLogger;

    /// <summary>
    /// 服务范围工厂, 每个入栈页在自己的范围里解析视图模型与仓储
    /// </summary>
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    /// <summary>
    /// 导航栈, 栈顶是当前覆盖在壳之上的那一页
    /// </summary>
    private readonly Stack<PageEntry> _navigationStack = new();

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
    /// 是否存在覆盖在壳之上的页面, 供视图控制导航栈覆盖层的显隐
    /// </summary>
    [ObservableProperty]
    public partial bool HasPage { get; set; }

    /// <summary>
    /// 当前覆盖在壳之上的页面视图模型, 导航栈为空时为 <see langword="null"/>
    /// </summary>
    [ObservableProperty]
    public partial IPageViewModel? CurrentPage { get; set; }

    /// <summary>
    /// 当前显示的确认对话框视图模型
    /// </summary>
    [ObservableProperty]
    public partial ConfirmViewModel? Confirm { get; set; }

    /// <summary>
    /// 确认对话框是否处于打开状态
    /// </summary>
    [ObservableProperty]
    public partial bool IsConfirmOpen { get; set; }

    /// <summary>
    /// 确认对话框是否可见
    /// </summary>
    [ObservableProperty]
    public partial bool IsConfirmVisible { get; set; }

    /// <summary>
    /// 确认对话框的淡入淡出透明度
    /// </summary>
    [ObservableProperty]
    public partial double ConfirmOpacity { get; set; }

    /// <summary>
    /// 确认对话框的纵向偏移
    /// </summary>
    [ObservableProperty]
    public partial double ConfirmOffsetY { get; set; } = ConfirmHiddenOffsetY;

    /// <summary>
    /// 按首页决策打开启动页
    /// </summary>
    public async void OpenHomePage()
    {
        try
        {
            if (uiOptions.CurrentTimetableId is not { } currentTimetableId)
            {
                await PushCoreAsync<TimetableListViewModel>(vm => vm.LoadAsync());
                return;
            }

            // TODO: 显示详细课表页
        }
        catch (Exception ex)
        {
            Toast.Show($"启动时载入课表失败: {ex.Message}");
            LogHomePageException(ex);
        }
    }

    /// <summary>
    /// 将一页推入导航栈
    /// </summary>
    /// <typeparam name="TPageViewModel">页面视图模型类型</typeparam>
    public Task PushAsync<TPageViewModel>() where TPageViewModel : IPageViewModel
    {
        return PushCoreAsync<TPageViewModel>(vm => vm.LoadAsync());
    }

    /// <summary>
    /// 将一页推入导航栈
    /// </summary>
    /// <typeparam name="TArg">页面载入参数类型</typeparam>
    /// <typeparam name="TPageViewModel">页面视图模型类型</typeparam>
    /// <param name="arg">页面载入参数</param>
    public Task PushAsync<TArg, TPageViewModel>(TArg arg) where TPageViewModel : IPageViewModel<TArg>
    {
        return PushCoreAsync<TPageViewModel>(vm => vm.LoadAsync(arg));
    }

    /// <summary>
    /// 请求显示确认对话框
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="message">说明文本</param>
    /// <param name="confirmText">确认按钮的文案</param>
    /// <param name="onConfirm">点击确认时执行的回调</param>
    public void RequestConfirm(string title, string message, string confirmText, Func<Task> onConfirm)
    {
        if (IsConfirmVisible) { return; }

        Confirm = new(title, message, confirmText, onConfirm, CloseConfirmAsync, Toast, confirmLogger);
        IsConfirmOpen = true;
        IsConfirmVisible = true;
        ConfirmOpacity = Constants.MaxRatio;
        ConfirmOffsetY = 0;
    }

    /// <summary>
    /// 请求取消确认对话框
    /// </summary>
    public void RequestCancelConfirm()
    {
        if (!IsConfirmVisible) { return; }
        _ = CloseConfirmAsync();
    }

    /// <summary>
    /// 处理一次返回请求, 依次尝试关闭确认对话框, 弹出导航栈与回退到课表 Tab
    /// </summary>
    /// <returns><see langword="true"/> 表示已消费, 否则为 <see langword="false"/></returns>
    public bool TryGoBack()
    {
        if (IsConfirmOpen)
        {
            _ = CloseConfirmAsync();
            return true;
        }

        if (_navigationStack.Count > 0)
        {
            var entry = _navigationStack.Pop();
            entry.Scope.Dispose();

            var hasPage = _navigationStack.Count > 0;
            HasPage = hasPage;
            CurrentPage = hasPage ? _navigationStack.Peek().ViewModel : null;

            LogPopped(_navigationStack.Count);
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
        return PushCoreAsync<TimetableListViewModel>(vm => vm.LoadAsync());
    }

    /// <summary>
    /// 打开新建课表页
    /// </summary>
    [RelayCommand]
    private Task OpenCreateTimetableAsync()
    {
        return PushCoreAsync<CreateTimetableViewModel>(vm => vm.LoadAsync());
    }

    /// <summary>
    /// 将一页推入导航栈的核心逻辑
    /// </summary>
    /// <typeparam name="TPageViewModel">页面视图模型类型</typeparam>
    /// <param name="load">加载页面数据的委托</param>
    private async Task PushCoreAsync<TPageViewModel>(Func<TPageViewModel, Task<Result>> load)
    where TPageViewModel : IPageViewModel
    {
        var scope = _scopeFactory.CreateScope();

        try
        {
            var viewModel = scope.ServiceProvider.GetRequiredService<TPageViewModel>();
            var result = await load(viewModel);

            if (result is FailureResult failure)
            {
                Toast.Show(failure.Message);
                scope.Dispose();

                LogPushFailed(typeof(TPageViewModel).Name, failure.Message, failure.Code);
                return;
            }

            Debug.Assert(result.IsSuccess, "载入结果必须是成功结果");
            HasPage = true;
            CurrentPage = viewModel;
            _navigationStack.Push(new(scope, viewModel));

            LogPushed(typeof(TPageViewModel).Name, _navigationStack.Count);
        }
        catch (Exception ex)
        {
            Toast.Show($"页面 {typeof(TPageViewModel).Name} 入栈失败: {ex.Message}");
            scope.Dispose();

            LogPushException(typeof(TPageViewModel).Name, ex);
        }
    }

    /// <summary>
    /// 关闭确认对话框
    /// </summary>
    private async Task CloseConfirmAsync()
    {
        IsConfirmOpen = false;
        ConfirmOpacity = 0;
        ConfirmOffsetY = ConfirmHiddenOffsetY;

        const int OverlayAnimationDurationMs = 200;
        await Task.Delay(OverlayAnimationDurationMs);

        IsConfirmVisible = false;
        Confirm = null;
    }

    /// <summary>
    /// 记录导航栈入栈失败的日志
    /// </summary>
    /// <param name="viewModelName">页面视图模型的类型名</param>
    /// <param name="errorMessage">入栈失败的错误信息</param>
    /// <param name="errorCode">入栈失败的错误码</param>
    [LoggerMessage(
        EventId = 1,
        EventName = "PushFailed",
        Level = LogLevel.Warning,
        Message = "Failed to push page: {ViewModelName}, Error={ErrorMessage}, Code={ErrorCode}"
    )]
    private partial void LogPushFailed(string viewModelName, string errorMessage, ErrorCode errorCode);

    /// <summary>
    /// 记录页面入栈完成的日志
    /// </summary>
    /// <param name="viewModelName">页面视图模型的类型名</param>
    /// <param name="depth">入栈后的栈深</param>
    [LoggerMessage(
        EventId = 2,
        EventName = "Pushed",
        Level = LogLevel.Debug,
        Message = "Pushed page: {ViewModelName}, Depth={Depth}"
    )]
    private partial void LogPushed(string viewModelName, int depth);

    /// <summary>
    /// 记录页面入栈发生异常的日志
    /// </summary>
    /// <param name="viewModelName">页面视图模型的类型名</param>
    /// <param name="exception">异常</param>
    [LoggerMessage(
        EventId = 3,
        EventName = "PushException",
        Level = LogLevel.Warning,
        Message = "Exception occurred while pushing page: {ViewModelName}"
    )]
    private partial void LogPushException(string viewModelName, Exception exception);

    /// <summary>
    /// 记录页面出栈完成的日志
    /// </summary>
    /// <param name="depth">出栈后的栈深</param>
    [LoggerMessage(
        EventId = 4,
        EventName = "Popped",
        Level = LogLevel.Debug,
        Message = "Popped page: Depth={Depth}"
    )]
    private partial void LogPopped(int depth);

    /// <summary>
    /// 记录启动时载入课表失败的日志
    /// </summary>
    /// <param name="exception">异常</param>
    [LoggerMessage(
        EventId = 5,
        EventName = "HomePageException",
        Level = LogLevel.Warning,
        Message = "Exception occurred while opening the home page"
    )]
    private partial void LogHomePageException(Exception exception);
}
