using Microsoft.Extensions.DependencyInjection;

namespace ClassSchedule.UI.Shared;

/// <summary>
/// UI 共享层服务的 DI 注册扩展方法
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> 类的扩展
    /// </summary>
    /// <param name="services">服务集合</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// 注册 UI 共享层的所有服务
        /// </summary>
        /// <returns>服务集合</returns>
        public IServiceCollection AddUIShared()
        {
            return services;
        }
    }
}
