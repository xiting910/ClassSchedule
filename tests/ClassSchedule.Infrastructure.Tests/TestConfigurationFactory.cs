using Microsoft.Extensions.Configuration;

namespace ClassSchedule.Infrastructure.Tests;

/// <summary>
/// 测试用配置对象工厂
/// </summary>
internal static class TestConfigurationFactory
{
    /// <summary>
    /// 根据键值对创建内存配置
    /// </summary>
    /// <param name="values">配置键值对</param>
    /// <returns>配置对象</returns>
    public static IConfiguration Create(params IEnumerable<(string Key, string? Value)> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(v => v.Key, v => v.Value))
            .Build();
    }
}
