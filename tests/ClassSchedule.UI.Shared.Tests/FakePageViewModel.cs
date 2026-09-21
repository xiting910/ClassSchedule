using ClassSchedule.Domain.Models;

#pragma warning disable IDE0130
namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 测试用页面视图模型, 载入行为由用例配置
/// </summary>
public sealed class FakePageViewModel : IPageViewModel<Guid>, IDisposable
{
    /// <summary>
    /// 载入时返回的结果, 为 <see langword="null"/> 时返回成功结果
    /// </summary>
    public Result? LoadResult { get; set; }

    /// <summary>
    /// 载入时抛出的异常, 为 <see langword="null"/> 时不抛出
    /// </summary>
    public Exception? LoadException { get; set; }

    /// <summary>
    /// 刷新时抛出的异常, 为 <see langword="null"/> 时不抛出
    /// </summary>
    public Exception? RefreshException { get; set; }

    /// <summary>
    /// 最近一次带参数载入时收到的参数
    /// </summary>
    public Guid? LoadArgument { get; private set; }

    /// <summary>
    /// 是否已被释放
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 刷新次数
    /// </summary>
    public int RefreshCount { get; private set; }

    /// <inheritdoc/>
    public Task<Result> LoadAsync()
    {
        return LoadException is null ? Task.FromResult(LoadResult ?? Result.Success()) : throw LoadException;
    }

    /// <inheritdoc/>
    public Task<Result> LoadAsync(Guid arg)
    {
        LoadArgument = arg;
        return LoadAsync();
    }

    /// <inheritdoc/>
    public Task RefreshAsync()
    {
        RefreshCount++;
        return RefreshException is null ? Task.CompletedTask : Task.FromException(RefreshException);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        IsDisposed = true;
    }
}
