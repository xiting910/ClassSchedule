using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ClassSchedule.Infrastructure;

/// <summary>
/// 文件系统类, 提供与文件系统相关的常量和方法
/// </summary>
public static class FileSystem
{
    /// <summary>
    /// 程序数据根目录的环境变量名
    /// </summary>
    public const string AppDataRootDirVariable = $"{nameof(ClassSchedule)}_Data_Root_Dir";

    /// <summary>
    /// JSON 文件后缀名
    /// </summary>
    public const string JsonFileSuffix = ".json";

    /// <summary>
    /// 日志后缀名
    /// </summary>
    public const string LogFileSuffix = ".log";

    /// <summary>
    /// 获取用于文件系统的字符串比较器
    /// </summary>
    public static StringComparer Comparer { get; }

    /// <summary>
    /// 获取程序数据根目录
    /// </summary>
    public static DirectoryInfo AppDataRootDirectory { get; }

    /// <summary>
    /// 获取数据目录
    /// </summary>
    public static DirectoryInfo Datas { get; }

    /// <summary>
    /// 获取程序日志目录
    /// </summary>
    public static DirectoryInfo Logs { get; }

    /// <summary>
    /// 获取程序设置目录
    /// </summary>
    public static DirectoryInfo Settings { get; }

    /// <summary>
    /// 获取用于 JSON 序列化和反序列化的选项
    /// </summary>
    public static JsonSerializerOptions JsonSerializerOptions { get; }

    /// <summary>
    /// 静态构造函数, 确保在第一次访问任何成员之前, 初始化相关的属性并创建必要的目录
    /// </summary>
    /// <remarks><para>
    /// 本方法中的 I/O 操作可能失败, 此时 CLR 会将异常包装为 <see cref="TypeInitializationException"/>
    /// 抛出, 之后对本类的任何访问都会再次抛出该异常, 进程内无法恢复
    /// <para></para>
    /// 这是刻意的 fail-fast 设计: 该方法失败说明用户的环境变量设置不正确或者程序没有足够的权限访问文件系统等等,
    /// 这些情况都无法在进程内恢复并且不是程序自身的问题, 应该由用户自行解决后重启程序
    /// <para></para>
    /// 本程序仅在 <see cref="UnhandledExceptionHelper"/> 中记录未处理异常日志, 以便用户可以查看异常信息
    /// </para></remarks>
    static FileSystem()
    {
        // 根据操作系统设置字符串比较器, Windows 系统使用不区分大小写的比较器, 其他系统使用区分大小写的比较器
        Comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        // 获取程序数据根目录, 如果环境变量未设置, 则使用默认路径
        var rootDir = Environment.GetEnvironmentVariable(AppDataRootDirVariable);
        if (string.IsNullOrWhiteSpace(rootDir))
        {
            rootDir = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData,
                    Environment.SpecialFolderOption.Create
                ), nameof(ClassSchedule)
            );
        }

        // 创建根目录
        AppDataRootDirectory = Directory.CreateDirectory(rootDir);

        // 创建数据目录
        Datas = AppDataRootDirectory.CreateSubdirectory(nameof(Datas));

        // 创建日志目录
        Logs = AppDataRootDirectory.CreateSubdirectory(nameof(Logs));

        // 创建设置目录
        Settings = AppDataRootDirectory.CreateSubdirectory(nameof(Settings));

        // 初始化 JSON 序列化选项, 设置为缩进格式, 允许尾随逗号, 并跳过注释
        JsonSerializerOptions = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
    }

    /// <summary>
    /// 将指定内容写入指定目录下的指定文件, 如果目录不存在, 则创建该目录
    /// </summary>
    /// <remarks>
    /// 该方法不会捕获任何异常, 所有的异常都会被外抛到调用方, 调用方需要自行处理异常
    /// </remarks>
    /// <param name="directory">指定的目录</param>
    /// <param name="fileName">要写入的文件名</param>
    /// <param name="content">要写入的内容</param>
    public static void WriteToFile(DirectoryInfo directory, string fileName, string content)
    {
        if (!directory.Exists)
        {
            directory.Create();
        }
        File.WriteAllText(Path.Combine(directory.FullName, fileName), content);
    }

    /// <summary>
    /// 如果存在日志文件夹, 则异步导出日志文件夹压缩包到指定流
    /// </summary>
    /// <remarks>
    /// 该方法不会捕获任何异常, 所有的异常都会被外抛到调用方, 调用方需要自行处理异常
    /// </remarks>
    /// <param name="stream">目标流</param>
    /// <param name="cancellationToken">取消令牌</param>
    public static async Task ExportLogsAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (Logs.Exists)
        {
            await using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, true);
            foreach (var logFile in Logs.EnumerateFiles($"*{LogFileSuffix}"))
            {
                await using var entryStream = zipArchive.CreateEntry(logFile.Name).Open();
                await using var logFileStream = logFile.FullName.OpenRead();
                await logFileStream.CopyToAsync(entryStream, cancellationToken);
            }
        }
    }

    /// <summary>
    /// <see cref="IConfigurationBuilder"/> 类的扩展
    /// </summary>
    /// <param name="builder">配置构建器</param>
    extension(IConfigurationBuilder builder)
    {
        /// <summary>
        /// 将 <see cref="Settings"/> 目录下的所有 JSON 文件添加到配置构建器中
        /// </summary>
        /// <returns>配置构建器</returns>
        public IConfigurationBuilder AddJsonFilesFromSettings()
        {
            // 如果设置目录存在, 则遍历所有的 JSON 文件并添加到配置构建器中
            if (Settings.Exists)
            {
                foreach (var file in Settings.EnumerateFiles($"*{JsonFileSuffix}"))
                {
                    _ = builder.AddJsonFile(file.FullName);
                }
            }

            // 返回配置构建器
            return builder;
        }
    }
}
