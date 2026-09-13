using ClassSchedule.Domain.Entities;

namespace ClassSchedule.Domain.Tests;

/// <summary>
/// <see cref="Course"/> 的单元测试
/// </summary>
public sealed class CourseTests
{
    /// <summary>
    /// 验证 <see cref="Course.Name"/> 在设置为 <see langword="null"/> 时抛出
    /// <see cref="ArgumentNullException"/>
    /// </summary>
    [Fact]
    public void Name_NullValue_ShouldThrowArgumentNullException()
    {
        var course = new Course { Id = Guid.NewGuid(), Name = "测试课程", Color = "#FF0000" };

        var ex = Assert.Throws<ArgumentNullException>(() => course.Name = null!);
        Assert.Equal(nameof(Course.Name), ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="Course.Name"/> 在设置为空字符串或空白字符串时抛出 <see cref="ArgumentException"/>
    /// </summary>
    /// <param name="invalidName">非法的课程名称</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_BlankValue_ShouldThrowArgumentException(string invalidName)
    {
        var course = new Course { Id = Guid.NewGuid(), Name = "测试课程", Color = "#FF0000" };

        var ex = Assert.Throws<ArgumentException>(() => course.Name = invalidName);
        Assert.Equal(nameof(Course.Name), ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="Course.Color"/> 在设置为 <see langword="null"/> 时抛出
    /// <see cref="ArgumentNullException"/>
    /// </summary>
    [Fact]
    public void Color_NullValue_ShouldThrowArgumentNullException()
    {
        var course = new Course { Id = Guid.NewGuid(), Name = "测试课程", Color = "#FF0000" };

        var ex = Assert.Throws<ArgumentNullException>(() => course.Color = null!);
        Assert.Equal(nameof(Course.Color), ex.ParamName);
    }

    /// <summary>
    /// 验证 <see cref="Course.Color"/> 在设置为空字符串或空白字符串时抛出 <see cref="ArgumentException"/>
    /// </summary>
    /// <param name="invalidColor">非法的课程颜色</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Color_BlankValue_ShouldThrowArgumentException(string invalidColor)
    {
        var course = new Course { Id = Guid.NewGuid(), Name = "测试课程", Color = "#FF0000" };

        var ex = Assert.Throws<ArgumentException>(() => course.Color = invalidColor);
        Assert.Equal(nameof(Course.Color), ex.ParamName);
    }
}
