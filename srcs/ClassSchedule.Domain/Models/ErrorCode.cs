using System.ComponentModel;

namespace ClassSchedule.Domain.Models;

/// <summary>
/// 错误码枚举, 用于表示不同类型的错误
/// </summary>
public enum ErrorCode
{
    /// <summary>
    /// 未知错误
    /// </summary>
    [Description("未知错误")]
    Unknown,
}
