using ClassSchedule.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassSchedule.Infrastructure.Configurations;

/// <summary>
/// <see cref="Course"/> 的实体配置
/// </summary>
internal static class CourseConfiguration
{
    /// <summary>
    /// 配置课程
    /// </summary>
    /// <param name="builder">课程的关联构建器</param>
    internal static void Configure(OwnedNavigationBuilder<Timetable, Course> builder)
    {
        _ = builder.Property(c => c.Id).ValueGeneratedNever();
        _ = builder.OwnsMany(c => c.Fragments, FragmentConfiguration.Configure);
    }
}
