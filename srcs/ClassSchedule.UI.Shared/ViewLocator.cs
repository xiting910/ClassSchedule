using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;

namespace ClassSchedule.UI.Shared;

/// <summary>
/// 视图定位器类
/// </summary>
public sealed class ViewLocator : IDataTemplate
{
    /// <summary>
    /// 视图模型后缀
    /// </summary>
    private const string ViewModelSuffix = "ViewModel";

    /// <summary>
    /// 视图后缀
    /// </summary>
    private const string ViewSuffix = "View";

    /// <inheritdoc/>
    public Control? Build(object? data)
    {
        if (data is null) { return null; }

        var dataTypeName = data.GetType().FullName;
        var viewTypeName = dataTypeName?.Replace(ViewModelSuffix, ViewSuffix, StringComparison.Ordinal);

        if (viewTypeName is not null)
        {
            var viewType = Type.GetType(viewTypeName);
            if (typeof(Control).IsAssignableFrom(viewType))
            {
                return (Control?)Activator.CreateInstance(viewType);
            }
        }

        return new TextBlock { Text = $"未找到视图: {dataTypeName}" };
    }

    /// <inheritdoc/>
    public bool Match(object? data)
    {
        return data?.GetType().Name.EndsWith(ViewModelSuffix, StringComparison.Ordinal) is true;
    }
}
