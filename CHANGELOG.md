# Changelog

本文件记录了项目的所有重要变更。每个版本的变更都应在发布时记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/).

---

## [Unreleased]

### Added
- 添加基础设施层单元测试, 覆盖文件日志、文件系统、字符串扩展与未处理异常辅助类
- 添加 UI 共享层单元测试, 覆盖 Toast、UI 选项、视图定位器与组合根等, 引入 Avalonia.Headless 无头测试环境
- 添加应用壳架构, 通过 IShellInitializer 按平台注入壳视图与壳窗口
- 添加 Toast 提示视图, 支持淡入滑入动画、倒计时进度条、悬停暂停与点击关闭
- 添加日志导出能力 (文件日志压缩包与未处理异常日志导出)
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

### Changed
- 文件写入不再静默吞异常, 改为显式处理并外抛 (SafeWriteToFile 更名为 WriteToFile)
- 优化文件日志轮转与写入: 空文件不轮转, UTF-8 无 BOM 编码并显式 Flush
- 未处理异常日志移入应用数据目录, 目录不可用时回退到程序基目录
- 为 UI 选项与文件日志选项添加结构化日志记录
- 升级 Avalonia 至 12.1.2 并简化 Dependabot 分组配置

### Fixed
- 修复文件日志丢失异常信息的问题, 异常文本现在会写入日志行

[Unreleased]: https://github.com/xiting910/ClassSchedule/commits/main
