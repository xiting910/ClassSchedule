using ClassSchedule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassSchedule.Infrastructure.Configurations;

/// <summary>
/// <see cref="Timetable"/> 的实体配置
/// </summary>
internal sealed class TimetableConfiguration : IEntityTypeConfiguration<Timetable>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Timetable> builder)
    {
        _ = builder.HasKey(t => t.Id);
        _ = builder.Property(t => t.Id).ValueGeneratedNever();
        _ = builder.OwnsMany(t => t.PeriodDefinitions, PeriodDefinitionConfiguration.Configure);
        _ = builder.OwnsMany(t => t.Courses, CourseConfiguration.Configure);
    }
}
