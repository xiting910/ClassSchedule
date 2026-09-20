using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;
using ClassSchedule.Infrastructure.Interfaces;
using ClassSchedule.UI.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 课表列表视图模型, 用于切换当前课表, 以及重命名或删除课表
/// </summary>
/// <param name="logger">日志记录器</param>
/// <param name="repository">课程表仓储</param>
/// <param name="navigationStack">导航栈</param>
/// <param name="overlayHost">浮层宿主视图模型</param>
/// <param name="toast">全局提示视图模型</param>
/// <param name="uiOptions">UI 配置</param>
public sealed partial class TimetableListViewModel(
    ILogger<TimetableListViewModel> logger,
    ITimetableRepository repository,
    NavigationStack navigationStack,
    OverlayHostViewModel overlayHost,
    ToastViewModel toast,
    UIOptions uiOptions
) : ObservableObject, IPageViewModel
{
    /// <summary>
    /// 列表行
    /// </summary>
    public ObservableCollection<TimetableListItem> Items { get; } = [];

    /// <summary>
    /// 是否没有任何课表, 供视图显示空态
    /// </summary>
    [ObservableProperty]
    public partial bool IsEmpty { get; set; } = true;

    /// <inheritdoc/>
    public async Task<Result> LoadAsync()
    {
        var summaries = await repository.ListAsync();

        Items.Clear();
        foreach (var summary in summaries)
        {
            Items.Add(new(summary) { IsCurrent = summary.Id == uiOptions.CurrentTimetableId });
        }

        IsEmpty = Items.Count == 0;
        LogLoaded(Items.Count);
        return Result.Success();
    }

    /// <summary>
    /// 将指定课表设为当前课表并关闭本页
    /// </summary>
    /// <param name="item">被选中的列表行</param>
    public void Select(TimetableListItem item)
    {
        uiOptions.CurrentTimetableId = item.Id;
        LogSelected(item.Id, item.Name);
        _ = navigationStack.TryPop();
    }

    /// <summary>
    /// 提交指定行的行内重命名
    /// </summary>
    /// <param name="item">目标列表行</param>
    public async Task CommitRenameAsync(TimetableListItem item)
    {
        var name = item.EditingName.Trim();
        if (string.IsNullOrEmpty(name))
        {
            toast.Show("课表名称不能为空白");
            return;
        }

        if (name == item.Name)
        {
            item.IsRenaming = false;
            return;
        }

        try
        {
            var result = await repository.GetAsync(item.Id);
            if (result is FailureResult failure)
            {
                toast.Show(failure.Message);
                return;
            }

            var timetable = ((SuccessResult<Timetable>)result).Value;
            timetable.Name = name;
            await repository.SaveAsync();

            item.Name = name;
            item.IsRenaming = false;
            LogRenamed(item.Id, name);
        }
        catch (Exception ex)
        {
            toast.Show($"重命名课表失败: {ex.Message}");
            LogRenameException(item.Id, ex);
        }
    }

    /// <summary>
    /// 请求删除指定课表, 二次确认后执行
    /// </summary>
    /// <param name="item">目标列表行</param>
    public void RequestDelete(TimetableListItem item)
    {
        overlayHost.OpenConfirmOverlay(
            "删除课表",
            $"「{item.Name}」中的课程与片段会一并删除, 且无法恢复",
            "删除",
            () => Delete(item)
        );
    }

    /// <summary>
    /// 关闭本页回到上一页
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        _ = navigationStack.TryPop();
    }

    /// <summary>
    /// 删除指定课表并就地刷新列表
    /// </summary>
    /// <param name="item">目标列表行</param>
    private async void Delete(TimetableListItem item)
    {
        try
        {
            var result = await repository.DeleteAsync(item.Id);
            if (result is FailureResult failure)
            {
                toast.Show(failure.Message);
                return;
            }

            if (uiOptions.CurrentTimetableId == item.Id)
            {
                uiOptions.CurrentTimetableId = null;
            }

            _ = Items.Remove(item);
            IsEmpty = Items.Count == 0;
            LogDeleted(item.Id);
        }
        catch (Exception ex)
        {
            toast.Show($"删除课表失败: {ex.Message}");
            LogDeleteException(item.Id, ex);
        }
    }

    /// <summary>
    /// 记录课表列表载入完成的日志
    /// </summary>
    /// <param name="count">列表行数</param>
    [LoggerMessage(
        EventId = 1,
        EventName = "Loaded",
        Level = LogLevel.Debug,
        Message = "Timetable list loaded: {Count} items"
    )]
    private partial void LogLoaded(int count);

    /// <summary>
    /// 记录当前课表切换的日志
    /// </summary>
    /// <param name="timetableId">课表 Id</param>
    /// <param name="name">课表名称</param>
    [LoggerMessage(
        EventId = 2,
        EventName = "Selected",
        Level = LogLevel.Information,
        Message = "Current timetable switched: {TimetableId}, Name={Name}"
    )]
    private partial void LogSelected(Guid timetableId, string name);

    /// <summary>
    /// 记录课表重命名完成的日志
    /// </summary>
    /// <param name="timetableId">课表 Id</param>
    /// <param name="name">新名称</param>
    [LoggerMessage(
        EventId = 3,
        EventName = "Renamed",
        Level = LogLevel.Information,
        Message = "Timetable renamed: {TimetableId}, Name={Name}"
    )]
    private partial void LogRenamed(Guid timetableId, string name);

    /// <summary>
    /// 记录课表重命名失败的日志
    /// </summary>
    /// <param name="timetableId">课表 Id</param>
    /// <param name="exception">异常</param>
    [LoggerMessage(
        EventId = 4,
        EventName = "RenameException",
        Level = LogLevel.Warning,
        Message = "Exception occurred while renaming timetable: {TimetableId}"
    )]
    private partial void LogRenameException(Guid timetableId, Exception exception);

    /// <summary>
    /// 记录课表删除完成的日志
    /// </summary>
    /// <param name="timetableId">课表 Id</param>
    [LoggerMessage(
        EventId = 5,
        EventName = "Deleted",
        Level = LogLevel.Information,
        Message = "Timetable deleted from list: {TimetableId}"
    )]
    private partial void LogDeleted(Guid timetableId);

    /// <summary>
    /// 记录课表删除失败的日志
    /// </summary>
    /// <param name="timetableId">课表 Id</param>
    /// <param name="exception">异常</param>
    [LoggerMessage(
        EventId = 6,
        EventName = "DeleteException",
        Level = LogLevel.Warning,
        Message = "Exception occurred while deleting timetable: {TimetableId}"
    )]
    private partial void LogDeleteException(Guid timetableId, Exception exception);
}
