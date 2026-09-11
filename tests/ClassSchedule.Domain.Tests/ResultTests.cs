using ClassSchedule.Domain.Models;

namespace ClassSchedule.Domain.Tests;

/// <summary>
/// <see cref="Result"/> 的单元测试类
/// </summary>
public sealed class ResultTests
{
    /// <summary>
    /// 测试 <see cref="Result.Success"/> 方法返回的结果是成功的
    /// </summary>
    [Fact]
    public void Success_ShouldReturnSuccessResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        _ = Assert.IsType<SuccessResult>(result);
    }

    /// <summary>
    /// 测试 <see cref="Result.Success{T}"/> 方法返回的结果是成功的, 并且包含正确的返回值
    /// </summary>
    [Fact]
    public void SuccessWithValue_ShouldReturnSuccessResultWithValue()
    {
        var value = 9;
        var result = Result.Success(value);

        Assert.True(result.IsSuccess);
        var intResult = Assert.IsType<SuccessResult<int>>(result);
        Assert.Equal(value, intResult.Value);
    }

    /// <summary>
    /// 测试 <see cref="Result.Failure"/> 方法返回的结果是失败的, 并且包含正确的错误信息
    /// </summary>
    [Fact]
    public void Failure_ShouldReturnFailureResultWithError()
    {
        var result = Result.Failure(ErrorCode.Unknown, "未知错误");

        Assert.False(result.IsSuccess);
        var failureResult = Assert.IsType<FailureResult>(result);
        Assert.Equal(ErrorCode.Unknown, failureResult.Code);
        Assert.Equal("未知错误", failureResult.Message);
    }
}
