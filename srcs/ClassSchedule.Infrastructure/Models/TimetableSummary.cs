using System;

namespace ClassSchedule.Infrastructure.Models;

/// <summary>
/// 课表摘要, 供列表展示
/// </summary>
/// <param name="Id">课表 Id</param>
/// <param name="Name">课表名称</param>
/// <param name="FirstMonday">学期第一周的星期一日期</param>
/// <param name="TotalWeeks">学期总周数</param>
public sealed record TimetableSummary(Guid Id, string Name, DateOnly FirstMonday, int TotalWeeks);
