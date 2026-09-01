# Changelog

本文件记录了项目的所有重要变更。每个版本的变更都应在发布时记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/).

---

## [Unreleased]

### Added
- 初始化项目工程骨架, 采用 Domain / Infrastructure / UI 三层分层架构
- 支持桌面端 (Windows / Linux / macOS) 与 Android 双平台
- 集成 Avalonia 12 UI 框架与 CommunityToolkit.Mvvm (MVVM)
- 添加三层单元测试项目 (xUnit v3 + Moq + MTP)
- 配置 GitHub Actions CI/CD、CodeQL 安全分析、Dependabot 依赖自动更新
- 采用集中包管理 (CPM) 统一管理 NuGet 包版本

[Unreleased]: https://github.com/xiting910/ClassSchedule/commits/main
