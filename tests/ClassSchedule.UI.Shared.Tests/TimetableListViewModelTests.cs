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
    /// <returns>服务容器</returns>
    private static ServiceProvider CreateProvider(ITimetableRepository repository)
    {
        return new ServiceCollection()
            .AddLogging()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddSingleton<UIOptions>()
            .AddSingleton<ShellViewModel>()
            .AddSingleton<ToastViewModel>()
            .AddSingleton(repository)
            .AddScoped<TimetableListViewModel>()
            .BuildServiceProvider(new ServiceProviderOptions
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

            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);

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

            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);

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

            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);

            Assert.True(viewModel.Items[0].IsCurrent);
            Assert.False(viewModel.Items[1].IsCurrent);

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
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);

            viewModel.Select(viewModel.Items[0]);

            Assert.Equal(viewModel.Items[0].Id, uiOptions.CurrentTimetableId);
            Assert.False(shell.HasPage);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证重命名时名称为空白会弹出提示, 不访问仓储也不退出重命名态
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
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);
            var item = viewModel.Items[0];
            item.EditingName = "   ";
            item.IsRenaming = true;

            await viewModel.CommitRenameAsync(item);

            Assert.Equal("课表名称不能为空白", Assert.Single(shell.Toast.Items).Message);
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
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);
            var item = viewModel.Items[0];
            item.EditingName = item.Name;
            item.IsRenaming = true;

            await viewModel.CommitRenameAsync(item);

            Assert.False(item.IsRenaming);
            Assert.Empty(shell.Toast.Items);
            repository.Verify(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证名称两端有空白时按去除后的名称保存并就地更新该行
    /// </summary>
    [Fact]
    public async Task CommitRenameAsync_名称两端有空白_按去除后的名称保存()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var summary = new TimetableSummary(Guid.NewGuid(), "甲课表", SampleMonday, 18);
            var timetable = Assert.IsType<SuccessResult<Timetable>>(
                Timetable.Create("甲课表", SampleMonday, 18)
            ).Value;
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync([summary]);
            _ = repository.Setup(x => x.GetAsync(summary.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(timetable));
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);
            var item = viewModel.Items[0];
            item.EditingName = "  新名称  ";
            item.IsRenaming = true;

            await viewModel.CommitRenameAsync(item);

            Assert.Equal("新名称", item.Name);
            Assert.Equal("新名称", timetable.Name);
            Assert.False(item.IsRenaming);
            repository.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证仓储取回失败时弹出提示, 保持原名称与重命名态
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
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);
            var item = viewModel.Items[0];
            item.EditingName = "新名称";
            item.IsRenaming = true;

            await viewModel.CommitRenameAsync(item);

            Assert.Equal(FailureMessage, Assert.Single(shell.Toast.Items).Message);
            Assert.Equal("甲课表", item.Name);
            Assert.True(item.IsRenaming);
            repository.Verify(x => x.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证仓储抛出异常时弹出提示, 保持原名称与重命名态
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
                .Returns(Task.FromException<Result>(new InvalidOperationException(ExceptionMessage)));
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);
            var item = viewModel.Items[0];
            item.EditingName = "新名称";
            item.IsRenaming = true;

            await viewModel.CommitRenameAsync(item);

            Assert.Equal($"重命名课表失败: {ExceptionMessage}", Assert.Single(shell.Toast.Items).Message);
            Assert.Equal("甲课表", item.Name);
            Assert.True(item.IsRenaming);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证请求删除时弹出二次确认, 未确认之前不访问仓储
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
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);

            viewModel.RequestDelete(viewModel.Items[0]);

            var confirm = Assert.IsType<ConfirmViewModel>(shell.Confirm);
            Assert.Equal("删除课表", confirm.Title);
            Assert.Equal("「甲课表」中的课程与片段会一并删除, 且无法恢复", confirm.Message);
            Assert.Equal("删除", confirm.ConfirmText);
            repository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证返回命令关闭本页
    /// </summary>
    [Fact]
    public async Task GoBackCommand_执行_关闭本页()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);

            viewModel.GoBackCommand.Execute(null);

            Assert.False(shell.HasPage);
            Assert.Null(shell.CurrentPage);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证确认之后删除该课表并就地移除列表行
    /// </summary>
    [Fact]
    public async Task DeleteAsync_确认之后_删除该课表并移除列表行()
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
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);
            var item = viewModel.Items[0];

            viewModel.RequestDelete(item);
            var confirm = Assert.IsType<ConfirmViewModel>(shell.Confirm);
            await confirm.ConfirmCommand.ExecuteAsync(null);

            _ = Assert.Single(viewModel.Items);
            Assert.Equal("乙课表", viewModel.Items[0].Name);
            Assert.False(viewModel.IsEmpty);
            repository.Verify(x => x.DeleteAsync(item.Id, It.IsAny<CancellationToken>()), Times.Once);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证删掉当前课表时把当前课表置空
    /// </summary>
    [Fact]
    public async Task DeleteAsync_删除当前课表_清空当前课表ID()
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
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);

            viewModel.RequestDelete(viewModel.Items[0]);
            var confirm = Assert.IsType<ConfirmViewModel>(shell.Confirm);
            await confirm.ConfirmCommand.ExecuteAsync(null);

            Assert.Null(uiOptions.CurrentTimetableId);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证删掉的不是当前课表时保留当前课表
    /// </summary>
    [Fact]
    public async Task DeleteAsync_删除非当前课表_保留当前课表ID()
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
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);

            viewModel.RequestDelete(viewModel.Items[1]);
            var confirm = Assert.IsType<ConfirmViewModel>(shell.Confirm);
            await confirm.ConfirmCommand.ExecuteAsync(null);

            Assert.Equal(current.Id, uiOptions.CurrentTimetableId);
            _ = Assert.Single(viewModel.Items);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证仓储删除失败时弹出提示且不移除列表行
    /// </summary>
    [Fact]
    public async Task DeleteAsync_仓储删除失败_弹出提示且不移除列表行()
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
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);
            var item = viewModel.Items[0];

            viewModel.RequestDelete(item);
            var confirm = Assert.IsType<ConfirmViewModel>(shell.Confirm);
            await confirm.ConfirmCommand.ExecuteAsync(null);

            Assert.Equal(FailureMessage, Assert.Single(shell.Toast.Items).Message);
            Assert.Contains(item, viewModel.Items);
            Assert.False(viewModel.IsEmpty);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证仓储删除抛出异常时弹出提示且不移除列表行
    /// </summary>
    [Fact]
    public async Task DeleteAsync_仓储抛异常_弹出提示且不移除列表行()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            _ = repository.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromException<Result>(new InvalidOperationException(ExceptionMessage)));
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);
            var item = viewModel.Items[0];

            viewModel.RequestDelete(item);
            var confirm = Assert.IsType<ConfirmViewModel>(shell.Confirm);
            await confirm.ConfirmCommand.ExecuteAsync(null);

            Assert.Equal($"删除课表失败: {ExceptionMessage}", Assert.Single(shell.Toast.Items).Message);
            Assert.Contains(item, viewModel.Items);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证删掉最后一张课表后显示空态
    /// </summary>
    [Fact]
    public async Task DeleteAsync_删掉最后一张课表_显示空态()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new(Guid.NewGuid(), "甲课表", SampleMonday, 18)]);
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<TimetableListViewModel>();
            var viewModel = Assert.IsType<TimetableListViewModel>(shell.CurrentPage);

            viewModel.RequestDelete(viewModel.Items[0]);
            var confirm = Assert.IsType<ConfirmViewModel>(shell.Confirm);
            await confirm.ConfirmCommand.ExecuteAsync(null);

            Assert.Empty(viewModel.Items);
            Assert.True(viewModel.IsEmpty);

            return 0;
        }, TestContext.Current.CancellationToken);
    }
}
