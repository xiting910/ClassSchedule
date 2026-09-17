using ClassSchedule.Domain.Models;
using System;
using System.Threading.Tasks;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 可推入导航栈的页面视图模型接口
/// </summary>
public interface IPageViewModel
{
    /// <summary>
    /// 异步加载页面数据, 由 <see cref="ShellViewModel"/> 在入栈前调用
    /// </summary>
    /// <returns>页面载入结果</returns>
    Task<Result> LoadAsync();
}

/// <summary>
/// 可推入导航栈且需要载入参数的页面视图模型接口
/// </summary>
/// <typeparam name="TArg">页面载入参数类型</typeparam>
public interface IPageViewModel<TArg> : IPageViewModel
{
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"></exception>
    Task<Result> IPageViewModel.LoadAsync()
    {
        throw new InvalidOperationException($"{GetType().FullName} requires load arguments");
    }

    /// <summary>
    /// 异步加载页面数据, 由 <see cref="ShellViewModel"/> 在入栈前调用
    /// </summary>
    /// <param name="arg">页面载入参数</param>
    /// <returns>页面载入结果</returns>
    Task<Result> LoadAsync(TArg arg);
}
