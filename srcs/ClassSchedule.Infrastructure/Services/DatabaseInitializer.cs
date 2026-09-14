using ClassSchedule.Infrastructure.Interfaces;
using ClassSchedule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClassSchedule.Infrastructure.Services;

/// <summary>
/// <see cref="IDatabaseInitializer"/> 的实现, 提供数据库初始化功能
/// </summary>
/// <param name="_logger">日志记录器</param>
/// <param name="_scopeFactory">服务范围工厂</param>
internal sealed partial class DatabaseInitializer(
    ILogger<DatabaseInitializer> _logger,
    IServiceScopeFactory _scopeFactory
) : IDatabaseInitializer
{
    /// <inheritdoc/>
    public void Initialize()
    {
        using var scope = _scopeFactory.CreateScope();

        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database;
        database.Migrate();

        var connectionString = database.GetConnectionString() ?? "Unknown";
        LogDatabaseInitialized(connectionString);
    }

    /// <summary>
    /// 记录数据库初始化完成的日志
    /// </summary>
    [LoggerMessage(
        EventId = 0,
        EventName = "DatabaseInitialized",
        Level = LogLevel.Information,
        Message = "数据库初始化完成: {ConnectionString}"
    )]
    private partial void LogDatabaseInitialized(string connectionString);
}
