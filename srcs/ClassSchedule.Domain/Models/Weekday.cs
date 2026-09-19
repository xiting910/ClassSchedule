using System.ComponentModel;

namespace ClassSchedule.Domain.Models;

/// <summary>
/// 表示一周中的某一天的枚举
/// </summary>
public enum Weekday
{
    /// <summary>
    /// 星期一
    /// </summary>
    [Description("星期一")]
    Monday = 1,

    /// <summary>
    /// 星期二
    /// </summary>
    [Description("星期二")]
    Tuesday = 2,

    /// <summary>
    /// 星期三
    /// </summary>
    [Description("星期三")]
    Wednesday = 3,

    /// <summary>
    /// 星期四
    /// </summary>
    [Description("星期四")]
    Thursday = 4,

    /// <summary>
    /// 星期五
    /// </summary>
    [Description("星期五")]
    Friday = 5,

    /// <summary>
    /// 星期六
    /// </summary>
    [Description("星期六")]
    Saturday = 6,

    /// <summary>
    /// 星期日
    /// </summary>
    [Description("星期日")]
    Sunday = 7,
}
