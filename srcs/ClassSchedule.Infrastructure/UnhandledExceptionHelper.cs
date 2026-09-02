using System;
using System.IO;
using System.Threading;

namespace ClassSchedule.Infrastructure;

/// <summary>
/// 未处理异常帮助类
/// </summary>
public static class UnhandledExceptionHelper
{
    /// <summary>
    /// 未处理异常日志文件名
    /// </summary>
    private const string UnhandledExceptionLogFileName = $"UnhandledException{FileSystem.LogFileSuffix}";

    /// <summary>
    /// 锁对象
    /// </summary>
    private static readonly Lock _lock = new();

    /// <summary>
    /// 记录未处理异常的日志文件路径
    /// </summary>
    public static string UnhandledExceptionLogFilePath { get; } = Path.Combine(
        FileSystem.AppDataRootDirectory.FullName,
        UnhandledExceptionLogFileName
    );

    /// <summary>
    /// 处理未处理异常
    /// </summary>
    /// <param name="isTerminating">是否即将终止</param>
    /// <param name="ex">未处理异常</param>
    public static void HandleException(bool isTerminating, Exception ex)
    {
        try
        {
            if (!FileSystem.AppDataRootDirectory.Exists)
            {
                FileSystem.AppDataRootDirectory.Create();
            }

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
}
