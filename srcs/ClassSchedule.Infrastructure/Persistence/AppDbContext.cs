using ClassSchedule.Domain.Entities;
using ClassSchedule.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace ClassSchedule.Infrastructure.Persistence;

/// <summary>
/// 应用数据库上下文
/// </summary>
/// <param name="options">上下文选项</param>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// 数据库文件名
    /// </summary>
    public const string DatabaseFileName = $"{nameof(ClassSchedule)}.db";

    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    internal static string ConnectionString =>
        field ??= $"Data Source={Path.Combine(FileSystem.Datas.FullName, DatabaseFileName)}";

    /// <summary>
    /// 课程表集合
    /// </summary>
    internal DbSet<Timetable> Timetables => Set<Timetable>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.ApplyConfiguration(_timetableConfiguration);
    }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.AddInterceptors(_materializationInterceptor);
    }

    /// <summary>
    /// 课程表实体配置
    /// </summary>
    private static readonly TimetableConfiguration _timetableConfiguration = new();

    /// <summary>
    /// 实体物化拦截器
    /// </summary>
    private static readonly MaterializationInterceptor _materializationInterceptor = new();
}
