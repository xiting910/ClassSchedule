using ClassSchedule.Infrastructure.Tests;
using Xunit.Sdk;
using Xunit.v3;

// 程序集级夹具: 设置数据目录环境变量并构建服务容器与数据库, 确保测试不触碰真实数据目录
[assembly: AssemblyFixture(typeof(TestEnvironmentFixture))]

// 文件系统测试共享同一数据根目录, 禁用并行避免测试间文件冲突
[assembly: Parallelization(Mode = ParallelMode.None)]
