using Avalonia.Threading;
using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;
using ClassSchedule.Infrastructure.Interfaces;
using ClassSchedule.Infrastructure.Models;
using ClassSchedule.UI.Shared.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="TimetableListViewModel"/> 的单元测试
/// </summary>
public sealed class TimetableListViewModelTests
{
    /// <summary>
    /// 仓储返回失败时的错误信息
    /// </summary>
    private const string FailureMessage = "课表不存在";

    /// <summary>
    /// 仓储抛出异常时的异常信息
    /// </summary>
    private const string ExceptionMessage = "读写时发生异常";

    /// <summary>
    /// 测试用的首周周一日期
    /// </summary>
    private static readonly DateOnly SampleMonday = new(2026, 8, 31);

    /// <summary>
    /// 创建服务容器
    /// </summary>
    /// <param name="repository">课程表仓储</param>
    /// <param name="configure">追加服务注册的委托</param>
    /// <returns>服务容器</returns>
    private static ServiceProvider CreateProvider(
        ITimetableRepository repository,
        Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddSingleton<UIOptions>()
            .AddSingleton<NavigationStack>()
            .AddSingleton<OverlayHostViewModel>()
            .AddSingleton<ShellViewModel>()
            .AddSingleton<ToastViewModel>()
            .AddSingleton(repository)
            .AddScoped<TimetableListViewModel>();

        configure?.Invoke(services);
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    /// <summary>
    /// 创建一个各方法都成功的课程表仓储, 用例可以再覆盖其中某个方法
    /// </summary>
    /// <returns>课程表仓储</returns>
    private static Mock<ITimetableRepository> CreateRepository()
    {
        var repository = new Mock<ITimetableRepository>();
        _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _ = repository.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(ErrorCode.TimetableNotFound, FailureMessage));
        _ = repository.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _ = repository.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return repository;
    }

    /// <summary>
    /// 验证载入时按仓储返回的顺序生成列表行
    /// </summary>
    [Fact]
    public async Task LoadAsync_有课表_按顺序生成列表行()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            var first = new TimetableSummary(Guid.NewGuid(), "甲课表", SampleMonday, 18);
            var second = new TimetableSummary(Guid.NewGuid(), "乙课表", SampleMonday, 18);
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([first, second]);
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);

            Assert.Equal(2, viewModel.Items.Count);
            Assert.Equal("甲课表", viewModel.Items[0].Name);
            Assert.Equal("乙课表", viewModel.Items[1].Name);
            Assert.False(viewModel.IsEmpty);
            Assert.Equal("2026-08-31 ~ 2027-01-03 · 共 18 周", viewModel.Items[0].RangeText);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证没有任何课表时显示空态
    /// </summary>
    [Fact]
    public async Task LoadAsync_无课表_显示空态()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);

            Assert.Empty(viewModel.Items);
            Assert.True(viewModel.IsEmpty);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证当前课表只标记命中的那一行
    /// </summary>
    [Fact]
    public async Task LoadAsync_当前课表命中_只标记该行()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var current = new TimetableSummary(Guid.NewGuid(), "甲课表", SampleMonday, 18);
            var other = new TimetableSummary(Guid.NewGuid(), "乙课表", SampleMonday, 18);
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([current, other]);
            using var provider = CreateProvider(repository.Object);
            var uiOptions = provider.GetRequiredService<UIOptions>();
            uiOptions.CurrentTimetableId = current.Id;
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);

            Assert.True(viewModel.Items[0].IsCurrent);
            Assert.False(viewModel.Items[1].IsCurrent);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证刷新按仓储的最新结果重建列表, 不依赖载入路径
    /// </summary>
    [Fact]
    public async Task RefreshAsync_仓储结果变化_重建列表()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);
            _ = Assert.Single(viewModel.Items);

            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([
                    new(Guid.NewGuid(), "甲课表", SampleMonday, 18),
                    new(Guid.NewGuid(), "乙课表", SampleMonday, 18)
                ]);
            await viewModel.RefreshAsync();

            Assert.Equal(2, viewModel.Items.Count);
            Assert.Equal("乙课表", viewModel.Items[1].Name);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证选中一行时写入当前课表并关闭本页
    /// </summary>
    [Fact]
    public async Task Select_选中一行_写入当前课表并关闭本页()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            using var provider = CreateProvider(repository.Object);
            var uiOptions = provider.GetRequiredService<UIOptions>();
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);

            viewModel.Select(viewModel.Items[0]);

            Assert.Equal(viewModel.Items[0].Id, uiOptions.CurrentTimetableId);
            Assert.False(shell.NavigationStack.HasPage);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证名称被改成空白时弹出提示且不触碰仓储
    /// </summary>
    [Fact]
    public async Task CommitRenameAsync_名称为空白_弹出提示且不重命名()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);
            var item = viewModel.Items[0];
            item.IsRenaming = true;
            item.EditingName = "   ";

            await viewModel.CommitRenameAsync(item);

            Assert.Equal("甲课表", item.Name);
            Assert.True(item.IsRenaming);
            repository.Verify(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证名称没有变化时直接退出重命名态, 不访问仓储
    /// </summary>
    [Fact]
    public async Task CommitRenameAsync_名称未变化_直接退出重命名态()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);
            var item = viewModel.Items[0];
            item.IsRenaming = true;
            item.EditingName = "甲课表";

            await viewModel.CommitRenameAsync(item);

            Assert.False(item.IsRenaming);
            repository.Verify(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证名称两端有空白时按去除空白后的名称保存
    /// </summary>
    [Fact]
    public async Task CommitRenameAsync_名称两端有空白_按去除后的名称保存()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            const int TotalWeeks = 18;
            var repository = CreateRepository();
            var summary = new TimetableSummary(Guid.NewGuid(), "甲课表", SampleMonday, TotalWeeks);
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([summary]);
            var timetable = Assert.IsType<SuccessResult<Timetable>>(
                Timetable.Create("甲课表", SampleMonday, TotalWeeks)
            ).Value;
            _ = repository.Setup(x => x.GetAsync(summary.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(timetable));
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);
            var item = viewModel.Items[0];
            item.IsRenaming = true;
            item.EditingName = "  新名称  ";

            await viewModel.CommitRenameAsync(item);

            Assert.Equal("新名称", item.Name);
            Assert.Equal("新名称", timetable.Name);
            Assert.False(item.IsRenaming);
            repository.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证仓储取回失败时弹出提示并保持重命名态
    /// </summary>
    [Fact]
    public async Task CommitRenameAsync_仓储取回失败_弹出提示且保持重命名态()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);
            var item = viewModel.Items[0];
            item.IsRenaming = true;
            item.EditingName = "新名称";

            await viewModel.CommitRenameAsync(item);

            Assert.Equal("甲课表", item.Name);
            Assert.True(item.IsRenaming);
            Assert.Equal(FailureMessage, Assert.Single(shell.Toast.Items).Message);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证仓储抛异常时弹出提示并保持原名称
    /// </summary>
    [Fact]
    public async Task CommitRenameAsync_仓储抛异常_弹出提示且保持原名称()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            _ = repository.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException(ExceptionMessage));
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);
            var item = viewModel.Items[0];
            item.IsRenaming = true;
            item.EditingName = "新名称";

            await viewModel.CommitRenameAsync(item);

            Assert.Equal("甲课表", item.Name);
            Assert.True(item.IsRenaming);
            Assert.Equal($"重命名课表失败: {ExceptionMessage}", Assert.Single(shell.Toast.Items).Message);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证请求删除时弹出二次确认, 且未确认之前不调用仓储
    /// </summary>
    [Fact]
    public async Task RequestDelete_请求删除_弹出二次确认且未确认时不删除()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);

            viewModel.RequestDelete(viewModel.Items[0]);

            var confirm = Assert.IsType<ConfirmViewModel>(shell.OverlayHost.Current);
            Assert.True(shell.OverlayHost.HasOverlay);
            Assert.Equal("删除课表", confirm.Title);
            repository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证返回命令执行后关闭本页
    /// </summary>
    [Fact]
    public async Task GoBackCommand_执行_关闭本页()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);

            viewModel.GoBackCommand.Execute(null);

            Assert.False(shell.NavigationStack.HasPage);
            Assert.Null(shell.NavigationStack.CurrentPage);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证新建课表后返回列表页, 列表页被刷新并出现新建的课表
    /// </summary>
    [Fact]
    public async Task OpenCreateTimetableCommand_新建课表后返回_列表页刷新并出现新课表()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            _ = repository.Setup(x => x.AddAsync(It.IsAny<Timetable>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            using var provider = CreateProvider(repository.Object, services => services
                .AddScoped<CreateTimetableViewModel>());
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var list = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);
            await shell.NavigationStack.PushAsync<CreateTimetableViewModel>();
            var create = Assert.IsType<CreateTimetableViewModel>(shell.NavigationStack.CurrentPage);

            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([
                    new(Guid.NewGuid(), "甲课表", SampleMonday, 18),
                    new(Guid.NewGuid(), "乙课表", SampleMonday, 18)
                ]);
            create.Name = "乙课表";
            create.FirstDay = new DateTimeOffset(2026, 8, 31, 0, 0, 0, TimeSpan.Zero);
            await create.SubmitCommand.ExecuteAsync(null);
            await Dispatcher.UIThread.InvokeAsync(() => { });

            Assert.Same(list, shell.NavigationStack.CurrentPage);
            Assert.Equal(2, list.Items.Count);
            Assert.Equal("乙课表", list.Items[1].Name);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证确认之后删除该课表并就地移除列表行
    /// </summary>
    [Fact]
    public async Task Delete_确认之后_删除该课表并移除列表行()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            var summary = new TimetableSummary(Guid.NewGuid(), "甲课表", SampleMonday, 18);
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([summary]);
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);
            var item = viewModel.Items[0];

            viewModel.RequestDelete(item);
            var confirm = Assert.IsType<ConfirmViewModel>(shell.OverlayHost.Current);
            confirm.ConfirmCommand.Execute(null);

            Assert.False(shell.OverlayHost.HasOverlay);
            Assert.Null(shell.OverlayHost.Current);
            repository.Verify(x => x.DeleteAsync(summary.Id, It.IsAny<CancellationToken>()), Times.Once);
            Assert.DoesNotContain(item, viewModel.Items);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证删除当前课表时把当前课表 ID 置空
    /// </summary>
    [Fact]
    public async Task Delete_删除当前课表_清空当前课表ID()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            var summary = new TimetableSummary(Guid.NewGuid(), "甲课表", SampleMonday, 18);
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([summary]);
            using var provider = CreateProvider(repository.Object);
            var uiOptions = provider.GetRequiredService<UIOptions>();
            uiOptions.CurrentTimetableId = summary.Id;
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);

            viewModel.RequestDelete(viewModel.Items[0]);
            Assert.IsType<ConfirmViewModel>(shell.OverlayHost.Current).ConfirmCommand.Execute(null);

            Assert.Null(uiOptions.CurrentTimetableId);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证删除非当前课表时保留当前课表 ID
    /// </summary>
    [Fact]
    public async Task Delete_删除非当前课表_保留当前课表ID()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var current = new TimetableSummary(Guid.NewGuid(), "甲课表", SampleMonday, 18);
            var other = new TimetableSummary(Guid.NewGuid(), "乙课表", SampleMonday, 18);
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([current, other]);
            using var provider = CreateProvider(repository.Object);
            var uiOptions = provider.GetRequiredService<UIOptions>();
            uiOptions.CurrentTimetableId = current.Id;
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);

            viewModel.RequestDelete(viewModel.Items[1]);
            Assert.IsType<ConfirmViewModel>(shell.OverlayHost.Current).ConfirmCommand.Execute(null);

            Assert.Equal(current.Id, uiOptions.CurrentTimetableId);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证仓储删除失败时弹出提示且不移除列表行
    /// </summary>
    [Fact]
    public async Task Delete_仓储删除失败_弹出提示且不移除列表行()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            _ = repository.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure(ErrorCode.TimetableNotFound, FailureMessage));
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);
            var item = viewModel.Items[0];

            viewModel.RequestDelete(item);
            Assert.IsType<ConfirmViewModel>(shell.OverlayHost.Current).ConfirmCommand.Execute(null);

            Assert.Contains(item, viewModel.Items);
            Assert.Equal(FailureMessage, Assert.Single(shell.Toast.Items).Message);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证仓储抛异常时弹出提示且不移除列表行
    /// </summary>
    [Fact]
    public async Task Delete_仓储抛异常_弹出提示且不移除列表行()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            _ = repository.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException(ExceptionMessage));
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);
            var item = viewModel.Items[0];

            viewModel.RequestDelete(item);
            Assert.IsType<ConfirmViewModel>(shell.OverlayHost.Current).ConfirmCommand.Execute(null);

            Assert.Contains(item, viewModel.Items);
            Assert.Equal($"删除课表失败: {ExceptionMessage}", Assert.Single(shell.Toast.Items).Message);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证删掉最后一张课表后显示空态
    /// </summary>
    [Fact]
    public async Task Delete_删掉最后一张课表_显示空态()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.NavigationStack.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.NavigationStack.CurrentPage);

            viewModel.RequestDelete(viewModel.Items[0]);
            var confirm = Assert.IsType<ConfirmViewModel>(shell.OverlayHost.Current);
            confirm.ConfirmCommand.Execute(null);

            Assert.False(shell.OverlayHost.HasOverlay);
            Assert.Null(shell.OverlayHost.Current);

            Assert.Empty(viewModel.Items);
            Assert.True(viewModel.IsEmpty);

            return 0;
        }, TestContext.Current.CancellationToken);
    }
}
