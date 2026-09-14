using ClassSchedule.Domain.Entities;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClassSchedule.Infrastructure.Persistence;

/// <summary>
/// 拦截器: 确保实体被物化后执行额外逻辑, 例如对 <see cref="Timetable"/> 的节次定义进行排序
/// </summary>
internal sealed class MaterializationInterceptor : IMaterializationInterceptor
{
    /// <inheritdoc/>
    public object InitializedInstance(MaterializationInterceptionData materializationData, object instance)
    {
        if (instance is Timetable timetable)
        {
            timetable.EnsurePeriodDefinitionsSorted();
        }

        return instance;
    }
}
