using ClassSchedule.UI.Shared.Tests;
using Xunit.Sdk;
using Xunit.v3;

// 程序集级夹具: 在第一个测试运行前启动 headless 会话并设置数据目录环境变量, 确保测试不触碰真实数据目录
[assembly: AssemblyFixture(typeof(TestEnvironmentFixture))]

// 测试共享 headless 会话与同一数据根目录, 禁用并行避免测试间相互干扰
[assembly: Parallelization(Mode = ParallelMode.None)]
