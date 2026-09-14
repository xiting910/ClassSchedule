using ClassSchedule.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassSchedule.Infrastructure.Configurations;

/// <summary>
/// <see cref="PeriodDefinition"/> 的实体配置
/// </summary>
internal static class PeriodDefinitionConfiguration
{
    /// <summary>
    /// 配置课程节次定义
    /// </summary>
    /// <param name="builder">课程节次定义的关联构建器</param>
    internal static void Configure(OwnedNavigationBuilder<Timetable, PeriodDefinition> builder)
    {
        _ = builder.HasKey($"{nameof(Timetable)}{nameof(Timetable.Id)}", nameof(PeriodDefinition.Ordinal));
        _ = builder.Property(p => p.Ordinal).ValueGeneratedNever();
        _ = builder.Ignore(p => p.Duration);
    }
}
