using ClassSchedule.Domain.Entities;
using ClassSchedule.Domain.Models;
using ClassSchedule.Infrastructure.Interfaces;
using ClassSchedule.UI.Shared.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="CreateTimetableViewModel"/> 的单元测试
/// </summary>
public sealed class CreateTimetableViewModelTests
{
    /// <summary>
    /// 仓储抛出异常时的异常信息
    /// </summary>
    private const string ExceptionMessage = "写入时发生异常";

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
            .AddScoped<CreateTimetableViewModel>()
            .BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
    }

    /// <summary>
    /// 创建一个新增成功的课程表仓储, 用例可以再覆盖其中某个方法
    /// </summary>
    /// <returns>课程表仓储</returns>
    private static Mock<ITimetableRepository> CreateRepository()
    {
        var repository = new Mock<ITimetableRepository>();
        _ = repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _ = repository.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(ErrorCode.TimetableNotFound, "课表不存在"));
        _ = repository.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _ = repository.Setup(x => x.AddAsync(It.IsAny<Timetable>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _ = repository.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return repository;
    }

    /// <summary>
    /// 填入合法的表单内容, 供只关心某个字段的用例在此基础上改坏一处
    /// </summary>
    /// <param name="viewModel">新建课表页的视图模型</param>
    private static void FillValidForm(CreateTimetableViewModel viewModel)
    {
        viewModel.Name = "2026 秋季学期";
        viewModel.FirstDay = new(new(2026, 8, 31));
        viewModel.TotalWeeks = 18;
    }

    /// <summary>
    /// 验证初次载入时填入十二节时间模板与默认总周数
    /// </summary>
    [Fact]
    public async Task LoadAsync_初次载入_填入十二节模板()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(CreateRepository().Object);
            var shell = provider.GetRequiredService<ShellViewModel>();

            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);

            Assert.Equal(12, viewModel.Periods.Count);
            Assert.Equal(1, viewModel.Periods[0].Ordinal);
            Assert.Equal("1.", viewModel.Periods[0].OrdinalText);
            Assert.Equal(12, viewModel.Periods[^1].Ordinal);
            Assert.Equal(new(8, 0), viewModel.Periods[0].StartTime);
            Assert.Equal(new(8, 45), viewModel.Periods[0].EndTime);
            Assert.Equal(new(21, 45), viewModel.Periods[^1].StartTime);
            Assert.Equal(new(22, 30), viewModel.Periods[^1].EndTime);
            Assert.Equal(18, viewModel.TotalWeeks);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证重复载入时先清空再按模板重填, 不会累积节次也不会留下上一次的改动
    /// </summary>
    [Fact]
    public async Task LoadAsync_重复调用_按模板重填而不累积()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(CreateRepository().Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            viewModel.Periods[0].StartTime = new(9, 0);

            _ = await viewModel.LoadAsync();

            Assert.Equal(12, viewModel.Periods.Count);
            Assert.Equal(12, viewModel.Periods[^1].Ordinal);
            Assert.Equal(new(8, 0), viewModel.Periods[0].StartTime);
            Assert.Equal(18, viewModel.TotalWeeks);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证移除中间一节之后行号重排为连续序号, 后面的节次整体前移
    /// </summary>
    [Fact]
    public async Task RemovePeriod_移除中间一节_行号重排()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(CreateRepository().Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            var removed = viewModel.Periods[1];

            viewModel.RemovePeriod(removed);

            Assert.Equal(11, viewModel.Periods.Count);
            Assert.DoesNotContain(removed, viewModel.Periods);
            Assert.Equal(Enumerable.Range(1, 11), viewModel.Periods.Select(row => row.Ordinal));
            Assert.Equal(new(10, 0), viewModel.Periods[1].StartTime);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证只剩一节时不允许再移除, 弹出提示且行数不变
    /// </summary>
    [Fact]
    public async Task RemovePeriod_只剩一节_弹出提示且不移除()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(CreateRepository().Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            while (viewModel.Periods.Count > 1)
            {
                viewModel.RemovePeriod(viewModel.Periods[^1]);
            }

            viewModel.RemovePeriod(viewModel.Periods[0]);

            _ = Assert.Single(viewModel.Periods);
            Assert.Equal("至少要保留一节", Assert.Single(shell.Toast.Items).Message);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证追加一节时按末节结束时间加课间与课长预填
    /// </summary>
    [Fact]
    public async Task AddPeriod_追加一节_按末节时间预填()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(CreateRepository().Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);

            viewModel.AddPeriodCommand.Execute(null);

            Assert.Equal(13, viewModel.Periods.Count);
            var added = viewModel.Periods[^1];
            Assert.Equal(13, added.Ordinal);
            Assert.Equal("13.", added.OrdinalText);
            Assert.Equal(new(22, 40), added.StartTime);
            Assert.Equal(new(23, 25), added.EndTime);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证末节结束时间加课间就跨午夜时, 新行的两个时间都留空而不是回绕
    /// </summary>
    [Fact]
    public async Task AddPeriod_末节结束加课间跨午夜_两个时间都留空()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(CreateRepository().Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            viewModel.Periods[^1].EndTime = new(23, 50);

            viewModel.AddPeriodCommand.Execute(null);

            var added = viewModel.Periods[^1];
            Assert.Null(added.StartTime);
            Assert.Null(added.EndTime);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证开始时间放得下但加上课长会跨午夜时, 只留空结束时间
    /// </summary>
    [Fact]
    public async Task AddPeriod_末节结束加课长跨午夜_只留空结束时间()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(CreateRepository().Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            viewModel.Periods[^1].EndTime = new(23, 5);

            viewModel.AddPeriodCommand.Execute(null);

            var added = viewModel.Periods[^1];
            Assert.Equal(new(23, 15), added.StartTime);
            Assert.Null(added.EndTime);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证末节结束时间没有填时, 新行的两个时间都留空
    /// </summary>
    [Fact]
    public async Task AddPeriod_末节结束时间未填_两个时间都留空()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(CreateRepository().Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            viewModel.Periods[^1].EndTime = null;

            viewModel.AddPeriodCommand.Execute(null);

            var added = viewModel.Periods[^1];
            Assert.Null(added.StartTime);
            Assert.Null(added.EndTime);

            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证达到节次上限时不允许再追加, 弹出提示且行数不变
    /// </summary>
    [Fact]
    public async Task AddPeriod_达到节次上限_弹出提示且不追加()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            using var provider = CreateProvider(CreateRepository().Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            while (viewModel.Periods.Count < Timetable.MaxPeriodDefinitions)
            {
                viewModel.AddPeriodCommand.Execute(null);
            }

            viewModel.AddPeriodCommand.Execute(null);

            Assert.Equal(Timetable.MaxPeriodDefinitions, viewModel.Periods.Count);
            Assert.Equal(
                $"节次数不能超过 {Timetable.MaxPeriodDefinitions}",
                Assert.Single(shell.Toast.Items).Message
            );

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证名称为空白时弹出提示且不创建
    /// </summary>
    [Fact]
    public async Task SubmitAsync_名称为空白_弹出提示且不创建()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            FillValidForm(viewModel);
            viewModel.Name = "   ";

            await viewModel.SubmitCommand.ExecuteAsync(null);

            Assert.Equal("课表名称不能为空白", Assert.Single(shell.Toast.Items).Message);
            repository.Verify(x => x.AddAsync(It.IsAny<Timetable>(), It.IsAny<CancellationToken>()), Times.Never);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证没有选择第一周日期时弹出提示且不创建
    /// </summary>
    [Fact]
    public async Task SubmitAsync_未选择日期_弹出提示且不创建()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            FillValidForm(viewModel);
            viewModel.FirstDay = null;

            await viewModel.SubmitCommand.ExecuteAsync(null);

            Assert.Equal("请选择第一周的日期", Assert.Single(shell.Toast.Items).Message);
            repository.Verify(x => x.AddAsync(It.IsAny<Timetable>(), It.IsAny<CancellationToken>()), Times.Never);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证总周数为空或超出上下限时弹出提示且不创建
    /// </summary>
    [Fact]
    public async Task SubmitAsync_总周数超出范围_弹出提示且不创建()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            FillValidForm(viewModel);
            int?[] invalidTotalWeeks = [null, 0, Timetable.MaxTotalWeeks + 1];

            foreach (var totalWeeks in invalidTotalWeeks)
            {
                viewModel.TotalWeeks = totalWeeks;

                await viewModel.SubmitCommand.ExecuteAsync(null);

                Assert.Equal(
                    $"总周数必须在 1 到 {Timetable.MaxTotalWeeks} 之间",
                    Assert.Single(shell.Toast.Items).Message
                );
                shell.Toast.Items.Clear();
            }

            repository.Verify(x => x.AddAsync(It.IsAny<Timetable>(), It.IsAny<CancellationToken>()), Times.Never);
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证某一节的时间没有填完整时弹出提示且不创建
    /// </summary>
    [Fact]
    public async Task SubmitAsync_某节时间未填完整_弹出提示且不创建()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            FillValidForm(viewModel);
            viewModel.Periods[2].EndTime = null;

            await viewModel.SubmitCommand.ExecuteAsync(null);

            Assert.Equal("第 3 节的时间未填完整", Assert.Single(shell.Toast.Items).Message);
            repository.Verify(x => x.AddAsync(It.IsAny<Timetable>(), It.IsAny<CancellationToken>()), Times.Never);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证结束时间不晚于开始时间时弹出提示且不创建
    /// </summary>
    [Fact]
    public async Task SubmitAsync_结束时间不晚于开始时间_弹出提示且不创建()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            FillValidForm(viewModel);
            viewModel.Periods[0].EndTime = viewModel.Periods[0].StartTime;

            await viewModel.SubmitCommand.ExecuteAsync(null);

            Assert.Equal("第 1 节的结束时间必须晚于开始时间", Assert.Single(shell.Toast.Items).Message);
            repository.Verify(x => x.AddAsync(It.IsAny<Timetable>(), It.IsAny<CancellationToken>()), Times.Never);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证按开始时间排序后相邻节次重叠时弹出提示且不创建
    /// </summary>
    [Fact]
    public async Task SubmitAsync_相邻节次重叠_弹出提示且不创建()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            FillValidForm(viewModel);
            viewModel.Periods[1].StartTime = new(8, 30);

            await viewModel.SubmitCommand.ExecuteAsync(null);

            Assert.Equal("第 2 节与其他节次的时间重叠", Assert.Single(shell.Toast.Items).Message);
            repository.Verify(x => x.AddAsync(It.IsAny<Timetable>(), It.IsAny<CancellationToken>()), Times.Never);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证表单合法时创建课表, 名称两端空白被去除, 节次与学期信息一并写入
    /// </summary>
    [Fact]
    public async Task SubmitAsync_表单合法_创建课表并写入节次()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var createdTimetables = new List<Timetable>();
            var repository = CreateRepository();
            _ = repository.Setup(x => x.AddAsync(It.IsAny<Timetable>(), It.IsAny<CancellationToken>()))
                .Callback<Timetable, CancellationToken>((timetable, _) => createdTimetables.Add(timetable))
                .Returns(Task.CompletedTask);
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            FillValidForm(viewModel);
            viewModel.Name = "  2026 秋季学期  ";

            await viewModel.SubmitCommand.ExecuteAsync(null);

            var created = Assert.Single(createdTimetables);
            Assert.Equal("2026 秋季学期", created.Name);
            Assert.Equal(new(2026, 8, 31), created.FirstMonday);
            Assert.Equal(18, created.TotalWeeks);
            Assert.Equal(12, created.PeriodDefinitions.Count);
            Assert.Equal("已创建课表「2026 秋季学期」, 共 18 周", Assert.Single(shell.Toast.Items).Message);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证原来没有当前课表时, 新建成功之后把新表设为当前课表并关闭本页
    /// </summary>
    [Fact]
    public async Task SubmitAsync_原本没有当前课表_新表设为当前课表并关闭本页()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var createdTimetables = new List<Timetable>();
            var repository = CreateRepository();
            _ = repository.Setup(x => x.AddAsync(It.IsAny<Timetable>(), It.IsAny<CancellationToken>()))
                .Callback<Timetable, CancellationToken>((timetable, _) => createdTimetables.Add(timetable))
                .Returns(Task.CompletedTask);
            using var provider = CreateProvider(repository.Object);
            var uiOptions = provider.GetRequiredService<UIOptions>();
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            FillValidForm(viewModel);

            await viewModel.SubmitCommand.ExecuteAsync(null);

            var created = Assert.Single(createdTimetables);
            Assert.Equal(created.Id, uiOptions.CurrentTimetableId);
            Assert.False(shell.HasPage);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证原来已有当前课表时, 新建不会顶掉它
    /// </summary>
    [Fact]
    public async Task SubmitAsync_已有当前课表_不覆盖当前课表()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var existingId = Guid.NewGuid();
            var repository = CreateRepository();
            using var provider = CreateProvider(repository.Object);
            var uiOptions = provider.GetRequiredService<UIOptions>();
            uiOptions.CurrentTimetableId = existingId;
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            FillValidForm(viewModel);

            await viewModel.SubmitCommand.ExecuteAsync(null);

            Assert.Equal(existingId, uiOptions.CurrentTimetableId);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证仓储抛异常时弹出提示, 不停留在失败状态也不关闭本页
    /// </summary>
    [Fact]
    public async Task SubmitAsync_仓储抛异常_弹出提示且停留在本页()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            _ = repository.Setup(x => x.AddAsync(It.IsAny<Timetable>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromException(new InvalidOperationException(ExceptionMessage)));
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);
            FillValidForm(viewModel);

            await viewModel.SubmitCommand.ExecuteAsync(null);

            Assert.Equal($"新建课表失败: {ExceptionMessage}", Assert.Single(shell.Toast.Items).Message);
            Assert.True(shell.HasPage);
            Assert.Same(viewModel, shell.CurrentPage);

            shell.Toast.Items.Clear();
            return 0;
        }, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// 验证取消命令关闭本页而不创建课表
    /// </summary>
    [Fact]
    public async Task CancelCommand_执行_关闭本页()
    {
        _ = await TestEnvironmentFixture.Session.Dispatch(async () =>
        {
            var repository = CreateRepository();
            using var provider = CreateProvider(repository.Object);
            var shell = provider.GetRequiredService<ShellViewModel>();
            await shell.PushAsync<CreateTimetableViewModel>();
            var viewModel = Assert.IsType<CreateTimetableViewModel>(shell.CurrentPage);

            viewModel.CancelCommand.Execute(null);

            Assert.False(shell.HasPage);
            Assert.Null(shell.CurrentPage);
            repository.Verify(x => x.AddAsync(It.IsAny<Timetable>(), It.IsAny<CancellationToken>()), Times.Never);

            return 0;
        }, TestContext.Current.CancellationToken);
    }
}
