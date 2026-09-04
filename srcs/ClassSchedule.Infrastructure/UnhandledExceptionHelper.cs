using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClassSchedule.Infrastructure;

/// <summary>
/// 未处理异常帮助类
/// </summary>
public static class UnhandledExceptionHelper
{
    /// <summary>
    /// 未处理异常日志文件名
    /// </summary>
    private const string UnhandledExceptionLogFileName = "UnhandledException.txt";

    /// <summary>
    /// 锁对象
    /// </summary>
    private static readonly Lock _lock = new();

    /// <summary>
    /// 获取记录未处理异常的日志文件路径
    /// </summary>
    public static string UnhandledExceptionLogFilePath { get; } = GetUnhandledExceptionLogFilePath();

    /// <summary>
    /// 处理未处理异常
    /// </summary>
    /// <param name="isTerminating">是否即将终止</param>
    /// <param name="ex">未处理异常</param>
    public static void HandleException(bool isTerminating, Exception ex)
    {
        try
        {
            lock (_lock)
            {
                File.AppendAllText(
                    UnhandledExceptionLogFilePath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} " +
                    (isTerminating ? "[Terminating] " : "[Non-Terminating] ") +
                    ex.ToString() + Environment.NewLine
                );
            }
        }
        catch { /* 忽略写日志时发生的异常 */ }
    }

    /// <summary>
    /// 如果存在未处理异常日志, 则异步导出未处理异常日志到指定流
    /// </summary>
    /// <remarks>
    /// 该方法不会捕获任何异常, 所有的异常都会被外抛到调用方, 调用方需要自行处理异常
    /// </remarks>
    /// <param name="stream">目标流</param>
    /// <param name="cancellationToken">取消令牌</param>
    public static async Task ExportLogAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (File.Exists(UnhandledExceptionLogFilePath))
        {
            await using var fileStream = UnhandledExceptionLogFilePath.OpenRead();
            await fileStream.CopyToAsync(stream, cancellationToken);
        }
    }

    /// <summary>
    /// 获取未处理异常日志文件路径
    /// </summary>
    /// <returns>未处理异常日志文件路径</returns>
    private static string GetUnhandledExceptionLogFilePath()
    {
        try
        {
            // 如果 FileSystem 可用, 则创建 AppDataRootDirectory
            if (!FileSystem.AppDataRootDirectory.Exists)
            {
                FileSystem.AppDataRootDirectory.Create();
            }

            // 返回 AppDataRootDirectory 下的 UnhandledExceptionLogFileName
            return Path.Combine(FileSystem.AppDataRootDirectory.FullName, UnhandledExceptionLogFileName);
        }
        catch
        {
            // 如果 FileSystem 异常, 则返回 AppContext.BaseDirectory 下的 UnhandledExceptionLogFileName
            // AppContext.BaseDirectory 在桌面端返回应用程序的根目录, 在安卓端返回应用内部 files 目录
            return Path.Combine(AppContext.BaseDirectory, UnhandledExceptionLogFileName);
        }
    }
}
