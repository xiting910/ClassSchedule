namespace ClassSchedule.Infrastructure.Interfaces;

/// <summary>
/// 数据库初始化器接口
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// 初始化数据库
    /// </summary>
    void Initialize();
}
