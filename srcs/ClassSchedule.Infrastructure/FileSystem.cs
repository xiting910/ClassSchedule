using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;

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
    /// 安全地将指定内容写入指定目录下的指定文件, 如果目录不存在, 则创建该目录
    /// </summary>
    /// <param name="directory">指定的目录</param>
    /// <param name="fileName">要写入的文件名</param>
    /// <param name="content">要写入的内容</param>
    public static void SafeWriteToFile(DirectoryInfo directory, string fileName, string content)
    {
        try
        {
            if (!directory.Exists)
            {
                directory.Create();
            }

            File.WriteAllText(Path.Combine(directory.FullName, fileName), content);
        }
        catch (Exception) { }
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
            // 遍历所有的设置文件, 将所有的设置文件加载到配置构建器中
            foreach (var file in Settings.EnumerateFiles($"*{JsonFileSuffix}"))
            {
                _ = builder.AddJsonFile(file.FullName);
            }

            // 构建配置对象并返回
            return builder;
        }
    }
}
