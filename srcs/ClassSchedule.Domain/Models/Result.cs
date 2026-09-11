namespace ClassSchedule.Domain.Models;

/// <summary>
/// 表示结果的抽象基类
/// </summary>
/// <param name="IsSuccess">是否成功</param>
public abstract record Result(bool IsSuccess)
{
    /// <summary>
    /// 创建一个成功的结果
    /// </summary>
    /// <returns>成功的结果</returns>
    public static Result Success()
    {
        return new SuccessResult();
    }

    /// <summary>
    /// 创建一个包含返回值的成功结果
    /// </summary>
    /// <typeparam name="T">返回值的类型</typeparam>
    /// <param name="value">返回值</param>
    /// <returns>包含返回值的成功结果</returns>
    public static Result Success<T>(T value)
    {
        return new SuccessResult<T>(value);
    }

    /// <summary>
    /// 创建一个失败的结果
    /// </summary>
    /// <param name="code">错误码</param>
    /// <param name="message">错误信息</param>
    /// <returns>失败的结果</returns>
    public static Result Failure(ErrorCode code, string message)
    {
        return new FailureResult(code, message);
    }
}

/// <summary>
/// 表示成功的结果
/// </summary>
public sealed record SuccessResult() : Result(true);

/// <summary>
/// 表示包含返回值的成功结果
/// </summary>
/// <typeparam name="T">返回值的类型</typeparam>
/// <param name="Value">返回值</param>
public sealed record SuccessResult<T>(T Value) : Result(true);

/// <summary>
/// 表示失败的结果
/// </summary>
/// <param name="Code">错误码</param>
/// <param name="Message">错误信息</param>
public sealed record FailureResult(ErrorCode Code, string Message) : Result(false);
