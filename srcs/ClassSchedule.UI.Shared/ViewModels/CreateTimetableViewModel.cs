using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;
using ClassSchedule.Infrastructure.Interfaces;
using ClassSchedule.UI.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ClassSchedule.UI.Shared.ViewModels;

/// <summary>
/// 新建课表视图模型, 用于创建新的课表
/// </summary>
/// <param name="logger">日志记录器</param>
/// <param name="repository">课程表仓储</param>
/// <param name="navigationStack">导航栈</param>
/// <param name="toast">全局提示视图模型</param>
/// <param name="uiOptions">UI 配置</param>
public sealed partial class CreateTimetableViewModel(
    ILogger<CreateTimetableViewModel> logger,
    ITimetableRepository repository,
    NavigationStack navigationStack,
    ToastViewModel toast,
    UIOptions uiOptions
) : ObservableObject, IPageViewModel
{
    /// <summary>
    /// 12 节时间模板
    /// </summary>
    private static readonly (TimeOnly startTime, TimeOnly endTime)[] DefaultPeriodTemplate =
    [
        (new(8, 0), new(8, 45)),
        (new(8, 55), new(9, 40)),
        (new(10, 0), new(10, 45)),
        (new(10, 55), new(11, 40)),
        (new(14, 0), new(14, 45)),
        (new(14, 55), new(15, 40)),
        (new(16, 0), new(16, 45)),
        (new(16, 55), new(17, 40)),
        (new(19, 0), new(19, 45)),
        (new(19, 55), new(20, 40)),
        (new(20, 50), new(21, 35)),
        (new(21, 45), new(22, 30))
    ];

    /// <summary>
    /// 课表名称
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    /// <summary>
    /// 第一周的任意一天日期
    /// </summary>
    [ObservableProperty]
    public partial DateTimeOffset? FirstDay { get; set; }

    /// <summary>
    /// 学期总周数
    /// </summary>
    [ObservableProperty]
    public partial int? TotalWeeks { get; set; }

    /// <summary>
    /// 节次时间输入行
    /// </summary>
    public ObservableCollection<PeriodInputRow> Periods { get; } = [];

    /// <inheritdoc/>
    public Task<Result> LoadAsync()
    {
        Periods.Clear();
        foreach (var (startTime, endTime) in DefaultPeriodTemplate)
        {
            Periods.Add(new(Periods.Count + 1, startTime, endTime));
        }

        const int DefaultTotalWeeks = 18;
        TotalWeeks = DefaultTotalWeeks;
        return Task.FromResult(Result.Success());
    }

    /// <summary>
    /// 移除指定的一节并重排行号
    /// </summary>
    /// <param name="row">被移除的行</param>
    public void RemovePeriod(PeriodInputRow row)
    {
        if (Periods.Count <= 1)
        {
            toast.Show("至少要保留一节");
            return;
        }

        _ = Periods.Remove(row);
        for (var i = 0; i < Periods.Count; i++)
        {
            Periods[i].Ordinal = i + 1;
        }
    }

    /// <summary>
    /// 追加一节, 默认接在最后一节之后
    /// </summary>
    [RelayCommand]
    private void AddPeriod()
    {
        if (Periods.Count >= Timetable.MaxPeriodDefinitions)
        {
            toast.Show($"节次数不能超过 {Timetable.MaxPeriodDefinitions}");
            return;
        }

        TimeOnly? startTime = null;
        TimeOnly? endTime = null;
        if (Periods.Count > 0 && Periods[^1].EndTime is { } lastEndTime)
        {
            const int DefaultPeriodMinutes = 45;
            const int DefaultBreakMinutes = 10;

            var start = lastEndTime.AddMinutes(DefaultBreakMinutes, out var wrappedDays);
            if (wrappedDays == 0)
            {
                startTime = start;
                var end = start.AddMinutes(DefaultPeriodMinutes, out wrappedDays);
                if (wrappedDays == 0)
                {
                    endTime = end;
                }
            }
        }

        Periods.Add(new(Periods.Count + 1, startTime, endTime));
    }

    /// <summary>
    /// 校验表单并创建课表
    /// </summary>
    [RelayCommand]
    private async Task SubmitAsync()
    {
        var name = Name.Trim();
        if (string.IsNullOrEmpty(name))
        {
            toast.Show("课表名称不能为空白");
            return;
        }

        if (FirstDay is not { } firstDay)
        {
            toast.Show("请选择第一周的日期");
            return;
        }

        if (TotalWeeks is not { } totalWeeks || totalWeeks < 1 || totalWeeks > Timetable.MaxTotalWeeks)
        {
            toast.Show($"总周数必须在 1 到 {Timetable.MaxTotalWeeks} 之间");
            return;
        }

        var periods = new List<(int ordinal, TimeOnly startTime, TimeOnly endTime)>(Periods.Count);
        foreach (var row in Periods)
        {
            if (row.StartTime is not { } startTime || row.EndTime is not { } endTime)
            {
                toast.Show($"第 {row.Ordinal} 节的时间未填完整");
                return;
            }

            if (endTime <= startTime)
            {
                toast.Show($"第 {row.Ordinal} 节的结束时间必须晚于开始时间");
                return;
            }

            periods.Add((row.Ordinal, startTime, endTime));
        }

        var lastEndTime = TimeOnly.MinValue;
        foreach (var (ordinal, startTime, endTime) in periods.OrderBy(p => p.startTime))
        {
            if (startTime < lastEndTime)
            {
                toast.Show($"第 {ordinal} 节与其他节次的时间重叠");
                return;
            }

            lastEndTime = endTime;
        }

        try
        {
            var createResult = Timetable.Create(name, DateOnly.FromDateTime(firstDay.DateTime), totalWeeks);
            if (createResult is FailureResult createFailure)
            {
                toast.Show(createFailure.Message);
                return;
            }

            var timetable = ((SuccessResult<Timetable>)createResult).Value;
            var addPeriodsResult = timetable.AddPeriodDefinitions(periods.Select(
                p => (p.startTime, p.endTime)
            ));

            if (addPeriodsResult is FailureResult addPeriodsFailure)
            {
                toast.Show(addPeriodsFailure.Message);
                return;
            }

            await repository.AddAsync(timetable);
            uiOptions.CurrentTimetableId ??= timetable.Id;

            navigationStack.Pop();
            toast.Show($"已创建课表「{timetable.Name}」, 共 {timetable.TotalWeeks} 周");
            LogCreated(timetable.Id, timetable.Name, timetable.TotalWeeks, periods.Count);
        }
        catch (Exception ex)
        {
            toast.Show($"新建课表失败: {ex.Message}");
            LogCreateException(ex);
        }
    }

    /// <summary>
    /// 放弃新建并返回上一页
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        navigationStack.Pop();
    }

    /// <summary>
    /// 记录课表创建完成的日志
    /// </summary>
    /// <param name="timetableId">课表 Id</param>
    /// <param name="name">课表名称</param>
    /// <param name="totalWeeks">总周数</param>
    /// <param name="periodCount">节次数</param>
    [LoggerMessage(
        EventId = 1,
        EventName = "Created",
        Level = LogLevel.Information,
        Message = "Timetable created: {TimetableId}, Name={Name}, TotalWeeks={TotalWeeks}, PeriodCount={PeriodCount}"
    )]
    private partial void LogCreated(Guid timetableId, string name, int totalWeeks, int periodCount);

    /// <summary>
    /// 记录课表创建失败的日志
    /// </summary>
    /// <param name="exception">异常</param>
    [LoggerMessage(
        EventId = 2,
        EventName = "CreateException",
        Level = LogLevel.Warning,
        Message = "Exception occurred while creating timetable"
    )]
    private partial void LogCreateException(Exception exception);
}
