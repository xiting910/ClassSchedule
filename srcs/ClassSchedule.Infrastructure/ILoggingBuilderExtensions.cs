using ClassSchedule.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ClassSchedule.Infrastructure;

/// <summary>
/// <see cref="ILoggingBuilder"/> 的扩展类
/// </summary>
public static class ILoggingBuilderExtensions
{
    /// <summary>
    /// <see cref="ILoggingBuilder"/> 类的扩展
    /// </summary>
    /// <param name="builder">日志构建器</param>
    extension(ILoggingBuilder builder)
    {
        /// <summary>
        /// 注册文件日志记录器
        /// </summary>
        public void AddFileLogger()
        {
            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>()
            );
        }
    }
}
