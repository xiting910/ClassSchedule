using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClassSchedule.Infrastructure.Persistence;

/// <summary>
/// <see cref="AppDbContext"/> 的设计时工厂
/// </summary>
public sealed class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc/>
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        return new(optionsBuilder.UseSqlite(AppDbContext.ConnectionString).Options);
    }
}
