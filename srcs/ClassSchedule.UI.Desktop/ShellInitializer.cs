using Avalonia.Controls.ApplicationLifetimes;
using ClassSchedule.UI.Shared;
using ClassSchedule.UI.Shared.ViewModels;

namespace ClassSchedule.UI.Desktop;

/// <summary>
/// 桌面端的应用程序壳视图的初始化器
/// </summary>
/// <param name="viewModel">壳视图模型</param>
internal sealed class ShellInitializer(ShellViewModel viewModel) : IShellInitializer
{
    /// <inheritdoc/>
    public void Initialize(IApplicationLifetime? lifetime)
    {
        if (lifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.MainWindow = new ShellWindow
            {
                DataContext = viewModel
            };
        }
        else
        {
            throw new ApplicationLifetimeNotSupportedException(lifetime);
        }
    }
}
