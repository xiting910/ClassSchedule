using ClassSchedule.Domain.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 导航栈类, 用于管理页面的导航
/// </summary>
/// <param name="scopeFactory">服务范围工厂</param>
/// <param name="logger">导航栈的日志记录器</param>
/// <param name="toast">全局提示视图模型</param>
#pragma warning disable CA1711 // 标识符应采用正确的后缀
public sealed partial class NavigationStack(
    IServiceScopeFactory scopeFactory,
    ILogger<NavigationStack> logger,
    ToastViewModel toast
) : ObservableObject
#pragma warning restore CA1711 // 标识符应采用正确的后缀
{
    /// <summary>
    /// 导航栈里的一页: 页面级服务范围与它的视图模型
    /// </summary>
    /// <param name="Scope">页面级服务范围, 出栈时释放</param>
    /// <param name="ViewModel">页面视图模型, 由容器在该范围内创建</param>
    private sealed record PageEntry(IServiceScope Scope, IPageViewModel ViewModel);

    /// <summary>
    /// 导航栈, 栈顶是当前页
    /// </summary>
    private readonly Stack<PageEntry> _navigationStack = new();

    /// <summary>
    /// 当前页面视图模型
    /// </summary>
    [ObservableProperty]
    public partial IPageViewModel? CurrentPage { get; set; }

    /// <summary>
    /// 当前是否存在页面
    /// </summary>
    [ObservableProperty]
    public partial bool HasPage { get; set; }

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
    /// <typeparam name="TPageViewModel">页面视图模型类型</typeparam>
    /// <typeparam name="TArg">页面载入参数类型</typeparam>
    /// <param name="arg">页面载入参数</param>
    public Task PushAsync<TPageViewModel, TArg>(TArg arg) where TPageViewModel : IPageViewModel<TArg>
    {
        return PushCoreAsync<TPageViewModel>(vm => vm.LoadAsync(arg));
    }

    /// <summary>
    /// 将一页从导航栈中弹出, 如果导航栈为空则不执行任何操作
    /// </summary>
    public async void Pop()
    {
        if (_navigationStack.Count > 0)
        {
            await PopAsync();
        }
    }

    /// <summary>
    /// 尝试将一页从导航栈中弹出
    /// </summary>
    /// <returns><see langword="true"/> 如果成功弹出一页, 否则为 <see langword="false"/></returns>
    public bool TryPop()
    {
        if (_navigationStack.Count == 0) { return false; }

        _ = PopAsync();
        return true;
    }

    /// <summary>
    /// 将一页推入导航栈的核心逻辑
    /// </summary>
    /// <typeparam name="TPageViewModel">页面视图模型类型</typeparam>
    /// <param name="load">加载页面数据的委托</param>
    private async Task PushCoreAsync<TPageViewModel>(Func<TPageViewModel, Task<Result>> load)
    where TPageViewModel : IPageViewModel
    {
        var scope = scopeFactory.CreateScope();

        try
        {
            var viewModel = scope.ServiceProvider.GetRequiredService<TPageViewModel>();
            var result = await load(viewModel);

            if (result is FailureResult failure)
            {
                toast.Show(failure.Message);
                scope.Dispose();

                LogPushFailed(typeof(TPageViewModel).Name, failure.Message, failure.Code);
                return;
            }

            Debug.Assert(result.IsSuccess, "The result should be success after checking for failure.");
            HasPage = true;
            CurrentPage = viewModel;
            _navigationStack.Push(new(scope, viewModel));

            LogPushed(typeof(TPageViewModel).Name, _navigationStack.Count);
        }
        catch (Exception ex)
        {
            toast.Show($"页面 {typeof(TPageViewModel).Name} 入栈失败: {ex.Message}");
            scope.Dispose();

            LogPushException(typeof(TPageViewModel).Name, ex);
        }
    }

    /// <summary>
    /// 将一页从导航栈中弹出, 并刷新上一页的数据
    /// </summary>
    private async Task PopAsync()
    {
        var entry = _navigationStack.Pop();
        HasPage = _navigationStack.Count > 0;

        if (HasPage)
        {
            CurrentPage = _navigationStack.Peek().ViewModel;

            try
            {
                await CurrentPage.RefreshAsync();
            }
            catch (Exception ex)
            {
                var viewModelName = CurrentPage.GetType().Name;
                toast.Show($"刷新页面 {viewModelName} 失败: {ex.Message}");
                LogRefreshException(viewModelName, ex);
            }
        }
        else
        {
            CurrentPage = null;
        }

        entry.Scope.Dispose();
        LogPopped(_navigationStack.Count);
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
    /// 记录页面刷新失败的日志
    /// </summary>
    /// <param name="viewModelName">页面视图模型的类型名</param>
    /// <param name="exception">异常</param>
    [LoggerMessage(
        EventId = 4,
        EventName = "RefreshException",
        Level = LogLevel.Warning,
        Message = "Exception occurred while refreshing page: {ViewModelName}"
    )]
    private partial void LogRefreshException(string viewModelName, Exception exception);

    /// <summary>
    /// 记录页面出栈完成的日志
    /// </summary>
    /// <param name="depth">出栈后的栈深</param>
    [LoggerMessage(
        EventId = 5,
        EventName = "Popped",
        Level = LogLevel.Debug,
        Message = "Popped page: Depth={Depth}"
    )]
    private partial void LogPopped(int depth);
}
