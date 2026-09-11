using System.ComponentModel;

namespace ClassSchedule.Domain.Tests;

/// <summary>
/// <see cref="EnumExtensions"/> 类的单元测试
/// </summary>
public sealed class EnumExtensionsTests
{
    /// <summary>
    /// 测试用枚举类型
    /// </summary>
    private enum TestEnum
    {
        /// <summary>
        /// 测试值1
        /// </summary>
        [Description("描述1")]
        Value1,

        /// <summary>
        /// 测试值2
        /// </summary>
        [Description("描述2")]
        Value2,

        /// <summary>
        /// 测试值3
        /// </summary>
        Value3
    }

    /// <summary>
    /// 测试 <see cref="EnumExtensions.GetDescription"/> 方法在枚举值存在
    /// <see cref="DescriptionAttribute"/> 时返回正确的描述
    /// </summary>
    [Fact]
    public void GetDescription_ShouldReturnCorrectDescription_WhenDescriptionAttributeExists()
    {
        var description1 = TestEnum.Value1.GetDescription();
        var description2 = TestEnum.Value2.GetDescription();

        Assert.Equal("描述1", description1);
        Assert.Equal("描述2", description2);
    }

    /// <summary>
    /// 测试 <see cref="EnumExtensions.GetDescription"/> 方法在枚举值不存在
    /// <see cref="DescriptionAttribute"/> 时返回枚举值的名称
    /// </summary>
    [Fact]
    public void GetDescription_ShouldReturnEnumName_WhenDescriptionAttributeDoesNotExist()
    {
        var description3 = TestEnum.Value3.GetDescription();

        Assert.Equal(nameof(TestEnum.Value3), description3);
    }
}
