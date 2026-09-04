using System.IO;

namespace ClassSchedule.Infrastructure;

/// <summary>
/// <see cref="string"/> 的扩展类
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 用于文件流的缓冲区大小
    /// </summary>
    private const int BufferSize = 4 * 1024;

    /// <summary>
    /// <see cref="string"/> 的扩展
    /// </summary>
    /// <param name="filePath">文件路径</param>
    extension(string filePath)
    {
        /// <summary>
        /// 打开文件的追加写入流
        /// </summary>
        /// <returns>文件流</returns>
        public FileStream OpenAppend()
        {
            return new(filePath, FileMode.Append, FileAccess.Write, FileShare.Read, BufferSize, true);
        }

        /// <summary>
        /// 打开文件的读取流
        /// </summary>
        /// <returns>文件流</returns>
        public FileStream OpenRead()
        {
            return new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, BufferSize, true);
        }
    }
}
