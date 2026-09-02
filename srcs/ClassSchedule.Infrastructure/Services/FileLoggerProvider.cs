using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ClassSchedule.Infrastructure.Services;

/// <summary>
/// <see cref="ILoggerProvider"/> 的实现, 提供文件日志记录器
/// </summary>
internal sealed class FileLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// 最新日志文件名
    /// </summary>
    private const string LatestLogFileName = $"latest{FileSystem.LogFileSuffix}";

    /// <summary>
    /// 时间提供器, 用于获取当前时间
    /// </summary>
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// 日志记录器选项
    /// </summary>
    private readonly FileLoggerOptions _options;

    /// <summary>
    /// 日志通道, 生产者是各 <see cref="FileLogger"/>, 消费者是后台写入任务
    /// </summary>
    private readonly Channel<string> _channel;

    /// <summary>
    /// 日志文件路径
    /// </summary>
    private readonly string _logFilePath;

    /// <summary>
    /// 后台写入任务, 将日志从通道中读取并写入日志文件
    /// </summary>
    private readonly Task _writeLogTask;

    /// <summary>
    /// 日志写入器, 用于将日志写入日志文件流
    /// </summary>
    private StreamWriter? _streamWriter;

    /// <summary>
    /// 日志文件流, 用于写入日志到文件
    /// </summary>
    private FileStream? _fileStream;

    /// <summary>
    /// 构造函数, 完成日志轮转
    /// </summary>
    /// <param name="timeProvider">时间提供器</param>
    /// <param name="options">日志记录器选项</param>
    public FileLoggerProvider(TimeProvider timeProvider, FileLoggerOptions options)
    {
        // 保存时间提供器
        _timeProvider = timeProvider;

        // 保存日志记录器选项
        _options = options;

        // 创建日志通道, 允许多个生产者同时写入, 但只允许一个消费者读取
        _channel = Channel.CreateUnbounded<string>(new() { SingleReader = true });

        try
        {
            // 如果最新日志文件所在的目录不存在, 则创建该目录
            if (!FileSystem.Logs.Exists)
            {
                FileSystem.Logs.Create();
            }

            // 设置最新日志文件的完整路径
            _logFilePath = Path.Combine(FileSystem.Logs.FullName, LatestLogFileName);

            // 轮转日志文件
            RotateLogFiles();
        }
        catch (Exception)
        {
            // 如果在获取日志文件路径时发生异常, 则将日志文件放在应用程序的根目录下
            _logFilePath = Path.Combine(AppContext.BaseDirectory, LatestLogFileName);
        }

        // 启动后台写入任务, 将日志从通道中读取并写入日志文件
        _writeLogTask = WriteLogToFileAsync();
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_channel.Writer, _timeProvider, _options, categoryName);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _ = _channel.Writer.TryComplete();
        _writeLogTask.ConfigureAwait(false).GetAwaiter().GetResult();
        _writeLogTask.Dispose();
        _streamWriter?.Dispose();
        _fileStream?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 轮转日志文件, 将非空的最新日志文件移动为按时间命名的文件, 并删除多余的旧文件
    /// </summary>
    private void RotateLogFiles()
    {
        try
        {
            // 获取最新日志文件的 FileInfo 对象
            var latestLogFileInfo = new FileInfo(_logFilePath);

            // 判断最新日志是否存在
            if (latestLogFileInfo.Exists)
            {
                // 打开日志文件的读取流
                using var stream = latestLogFileInfo.OpenRead();

                // 如果读到任何内容, 则说明最新日志文件不为空, 需要轮转日志文件
                if (stream.ReadByte() != -1)
                {
                    // 轮转日志文件: 将最新日志文件移动到以当前时间命名的文件中
                    latestLogFileInfo.MoveTo(Path.Combine(
                        FileSystem.Logs.FullName,
                        $"{_timeProvider.GetLocalNow():yyyy-MM-dd_HHmmss}{FileSystem.LogFileSuffix}"
                    ));
                }
            }

            // 获取所有旧的日志文件, 按时间降序排序, 并跳过最新的 N 个文件
            var oldFiles = FileSystem.Logs
                .EnumerateFiles($"*{FileSystem.LogFileSuffix}", SearchOption.TopDirectoryOnly)
                .Where(path => !FileSystem.Comparer.Equals(path.Name, LatestLogFileName))
                .OrderByDescending(static path => path.Name)
                .Skip(_options.MaxLogFileCount - 1)
                .ToList();

            // 删除旧的日志文件
            oldFiles.ForEach(file => file.Delete());
        }
        catch { /* 忽略日志轮转过程中的异常 */ }
    }

    /// <summary>
    /// 后台写入任务, 将日志从通道中读取并写入日志文件
    /// </summary>
    private async Task WriteLogToFileAsync()
    {
        const int BufferSize = 4096;
        await foreach (var line in _channel.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            try
            {
                _fileStream ??= new(
                    _logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, BufferSize, true
                );
                _streamWriter ??= new(_fileStream) { AutoFlush = true };
                await _streamWriter.WriteLineAsync(line).ConfigureAwait(false);
            }
            catch { /* 忽略写入日志文件时的异常 */ }
        }
    }

    /// <summary>
    /// <see cref="ILogger"/> 的实现, 记录日志到文件
    /// </summary>
    /// <param name="_channelWriter">日志通道写入器</param>
    /// <param name="_timeProvider">时间提供器</param>
    /// <param name="_options">日志记录器选项</param>
    /// <param name="_categoryName">日志类别名称</param>
    private sealed class FileLogger(
        ChannelWriter<string> _channelWriter,
        TimeProvider _timeProvider,
        FileLoggerOptions _options,
        string _categoryName
    ) : ILogger
    {
        /// <inheritdoc/>
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            // 不支持日志作用域
            return null;
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel is not LogLevel.None && logLevel >= _options.MinLevel;
        }

        /// <inheritdoc/>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                // 拼接日志行: 时间戳, 级别缩写, 类别名称, 事件ID, 事件名, 日志内容, 异常信息
                var line = $"[{_timeProvider.GetLocalNow():yyyy-MM-dd HH:mm:ss.fff}] "
                    + $"[{logLevel}] [{_categoryName}] ({eventId.Id} {eventId.Name}) "
                    + formatter(state, exception);

                // 写入日志到通道
                _ = _channelWriter.TryWrite(line);
            }
        }
    }
}
