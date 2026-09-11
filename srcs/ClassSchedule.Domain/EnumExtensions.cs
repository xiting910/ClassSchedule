using System;
using System.ComponentModel;
using System.Reflection;

namespace ClassSchedule.Domain;

/// <summary>
/// 枚举扩展方法类
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// 枚举类的扩展块
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    /// <param name="enum">枚举值</param>
    extension<TEnum>(TEnum @enum) where TEnum : struct, Enum
    {
        /// <summary>
        /// 获取枚举值的描述
        /// </summary>
        /// <returns>描述文本</returns>
        public string GetDescription()
        {
            var s = @enum.ToString();
            return typeof(TEnum).GetField(s)?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? s;
        }
    }
}
