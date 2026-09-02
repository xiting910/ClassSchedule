# Changelog

本文件记录了项目的所有重要变更。每个版本的变更都应在发布时记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/).

---

## [Unreleased]

### Added
- 添加文件日志支持, 通过 Channel 异步写入, 按时间轮转并自动清理旧日志
- 添加未处理异常落盘记录, 桌面/Android 入口与 UI 线程异常均接入
- 添加应用数据目录管理, 支持环境变量自定义根目录与 JSON 设置的加载/保存
- 添加各层 DI 注册扩展 (AddDomain/AddInfrastructure/AddUIShared)
- 添加 UI 选项与主题模式, 支持跟随系统/浅色/深色并持久化设置
- 添加 Toast 提示模型与视图模型, 支持多条目、倒计时进度条与点击回调
- 初始化项目工程骨架, 采用 Domain / Infrastructure / UI 三层分层架构
- 支持桌面端 (Windows / Linux / macOS) 与 Android 双平台
- 集成 Avalonia 12 UI 框架与 CommunityToolkit.Mvvm (MVVM)
- 添加三层单元测试项目 (xUnit v3 + Moq + MTP)
- 配置 GitHub Actions CI/CD、CodeQL 安全分析、Dependabot 依赖自动更新
- 采用集中包管理 (CPM) 统一管理 NuGet 包版本

[Unreleased]: https://github.com/xiting910/ClassSchedule/commits/main
