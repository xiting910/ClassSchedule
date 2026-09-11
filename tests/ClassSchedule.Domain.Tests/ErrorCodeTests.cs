using ClassSchedule.Domain.Models;

namespace ClassSchedule.Domain.Tests;

/// <summary>
/// <see cref="ErrorCode"/> 的单元测试类
/// </summary>
public sealed class ErrorCodeTests
{
    /// <summary>
    /// 验证 <see cref="ErrorCode"/> 的所有枚举值的 <see cref="EnumExtensions.GetDescription"/>
    /// 方法都应该返回不等于枚举值名称的描述文本, 以确保每个错误码都有对应的描述
    /// </summary>
    [Fact]
    public void AllErrorCodes_GetDescription_ShouldNotReturnEnumName()
    {
        foreach (var code in Enum.GetValues<ErrorCode>())
        {
            Assert.NotEqual(code.ToString(), code.GetDescription());
        }
    }
}
