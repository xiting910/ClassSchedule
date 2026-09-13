<div align="center">

# 📅 ClassSchedule

📚 基于 **Avalonia UI** 的跨平台课程表应用 — 桌面 + Android 双平台

采用**分层架构**，使用 **.NET 10.0** 构建。

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![Avalonia 12](https://img.shields.io/badge/Avalonia-12-8B45C6.svg)](https://avaloniaui.net/)
[![Release](https://img.shields.io/github/v/release/xiting910/ClassSchedule)](https://github.com/xiting910/ClassSchedule/releases)
[![Downloads](https://img.shields.io/github/downloads/xiting910/ClassSchedule/total)](https://github.com/xiting910/ClassSchedule/releases)

[![Android](https://img.shields.io/badge/Android-3DDC84?logo=android&logoColor=white)](https://github.com/xiting910/ClassSchedule/releases)
[![Windows x64](https://img.shields.io/badge/Windows-x64-0078D6?logo=windows&logoColor=white)](https://github.com/xiting910/ClassSchedule/releases)
[![Linux x64](https://img.shields.io/badge/Linux-x64-FCC624?logo=linux&logoColor=black)](https://github.com/xiting910/ClassSchedule/releases)
[![macOS x64](https://img.shields.io/badge/macOS-x64-000000?logo=apple&logoColor=white)](https://github.com/xiting910/ClassSchedule/releases)

[![CI](https://github.com/xiting910/ClassSchedule/actions/workflows/ci.yml/badge.svg)](https://github.com/xiting910/ClassSchedule/actions/workflows/ci.yml)
[![CodeQL](https://github.com/xiting910/ClassSchedule/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/xiting910/ClassSchedule/actions/workflows/codeql-analysis.yml)
[![Dependency Review](https://github.com/xiting910/ClassSchedule/actions/workflows/dependency-review.yml/badge.svg)](https://github.com/xiting910/ClassSchedule/actions/workflows/dependency-review.yml)

</div>

---

## ✨ 特性

> 📌 项目处于早期开发阶段, 业务功能开发中, 当前已具备以下工程基础:

### 🔧 工程与架构

- 🧱 **分层架构** — Domain / Infrastructure / UI 三层分离, 高内聚低耦合
- 📱 **多平台支持** — 桌面端 (Windows / Linux / macOS) + Android, UI 代码跨端共享
- 🧩 **MVVM 模式** — CommunityToolkit.Mvvm 源代码生成器
- 🧪 **单元测试** — 三层测试项目 (xUnit v3 + Moq + MTP)
- 🔁 **CI/CD 自动化** — 自动构建、测试、CodeQL 分析、Release 发布
- 📦 **依赖自动更新** — Dependabot

---

## 🛠️ 技术栈

| 类别    | 技术                                                  |
| ------- | ----------------------------------------------------- |
| 运行时  | .NET 10 · C# 14                                       |
| UI 框架 | Avalonia 12 · CommunityToolkit.Mvvm                   |
| 架构    | 分层架构 (Domain / Infrastructure / UI) · MVVM        |
| 测试    | xUnit v3 · Moq · Avalonia.Headless                    |
| 工程化  | GitHub Actions · CodeQL · Dependabot · CPM 集中包管理 |

---

## 🏗️ 项目结构

```
ClassSchedule/
├── .github/                                                    # GitHub 配置
│   ├── ISSUE_TEMPLATE/                                         #  Issue 模板
│   │   ├── bug_report.md                                       #   Bug 报告模板
│   │   ├── config.yml                                          #   Issue 模板选择配置
│   │   └── feature_request.md                                  #   功能建议模板
│   ├── workflows/                                              #  GitHub Actions 工作流
│   │   ├── ci.yml                                              #   CI 构建与测试
│   │   ├── codeql-analysis.yml                                 #   CodeQL 安全分析
│   │   ├── dependabot-auto-merge.yml                           #   Dependabot PR 自动 approve + squash 合并
│   │   ├── dependency-review.yml                               #   依赖漏洞审查
│   │   ├── dependency-submission.yml                           #   依赖快照提交
│   │   ├── release-delete.yml                                  #   Release 清理
│   │   └── release-publish.yml                                 #   Release 发布
│   ├── dependabot.yml                                          #  Dependabot 依赖更新
│   └── PULL_REQUEST_TEMPLATE.md                                #  PR 描述模板
├── srcs/                                                       # 源码目录
│   ├── ClassSchedule.Domain/                                   #  领域层
│   │   ├── Entities/                                           #   领域实体
│   │   │   ├── Course.cs                                       #    课程实体
│   │   │   ├── Fragment.cs                                     #    课程片段实体
│   │   │   ├── PeriodDefinition.cs                             #    课程节次定义实体
│   │   │   ├── Timetable.cs                                    #    课程表实体
│   │   │   ├── Timetable.Course.cs                             #    课程表课程操作
│   │   │   ├── Timetable.Fragment.cs                           #    课程表片段操作
│   │   │   └── Timetable.Period.cs                             #    课程表节次操作
│   │   ├── Models/                                             #   领域模型
│   │   │   ├── ErrorCode.cs                                    #    错误码枚举
│   │   │   ├── OrdinalRange.cs                                 #    序数闭区间模型
│   │   │   ├── Result.cs                                       #    结果类型层次结构
│   │   │   └── Weekday.cs                                      #    星期枚举
│   │   ├── ClassSchedule.Domain.csproj                         #   项目文件
│   │   ├── DateOnlyExtensions.cs                               #   DateOnly 扩展成员
│   │   ├── DayOfWeekExtensions.cs                              #   DayOfWeek 扩展成员
│   │   └── EnumExtensions.cs                                   #   枚举扩展成员
│   ├── ClassSchedule.Infrastructure/                           #  基础设施层
│   │   ├── Services/                                           #   服务实现
│   │   │   └── FileLoggerProvider.cs                           #    文件日志提供器
│   │   ├── ClassSchedule.Infrastructure.csproj                 #   项目文件
│   │   ├── FileLoggerOptions.cs                                #   文件日志选项
│   │   ├── FileSystem.cs                                       #   文件系统路径与文件读写
│   │   ├── ILoggingBuilderExtensions.cs                        #   文件日志注册扩展
│   │   ├── IServiceCollectionExtensions.cs                     #   DI 注册扩展
│   │   ├── StringExtensions.cs                                 #   string 扩展成员
│   │   └── UnhandledExceptionHelper.cs                         #   未处理异常日志
│   ├── ClassSchedule.UI.Android/                               #  安卓端
│   │   ├── Properties/                                         #   Android 属性
│   │   │   └── AndroidManifest.xml                             #    Android 清单
│   │   ├── Resources/                                          #   Android 资源
│   │   │   ├── drawable/                                       #    位图与可绘制资源
│   │   │   │   ├── icon.png                                    #     应用图标
│   │   │   │   └── splash_screen.xml                           #     启动屏幕
│   │   │   ├── drawable-night-v31/                             #    深色模式动画
│   │   │   │   └── avalonia_anim.xml                           #     Avalonia 启动动画
│   │   │   ├── drawable-v31/                                   #    Android 12+ 动画
│   │   │   │   └── avalonia_anim.xml                           #     Avalonia 启动动画
│   │   │   ├── values/                                         #    资源值
│   │   │   │   ├── colors.xml                                  #     颜色定义
│   │   │   │   └── styles.xml                                  #     主题样式
│   │   │   ├── values-night/                                   #    深色模式颜色
│   │   │   │   └── colors.xml                                  #     颜色定义
│   │   │   └── values-v31/                                     #    Android 12+ 样式
│   │   │       └── styles.xml                                  #     主题样式
│   │   ├── ClassSchedule.UI.Android.csproj                     #   项目文件
│   │   ├── MainActivity.cs                                     #   主活动
│   │   ├── MainApplication.cs                                  #   主应用程序
│   │   └── ShellInitializer.cs                                 #   壳视图初始化器
│   ├── ClassSchedule.UI.Desktop/                               #  桌面端
│   │   ├── Assets/                                             #   资源目录
│   │   │   └── logo.ico                                        #    应用图标
│   │   ├── App.Manifest.xml                                    #   Windows 应用清单
│   │   ├── ClassSchedule.UI.Desktop.csproj                     #   项目文件
│   │   ├── Program.cs                                          #   应用入口
│   │   ├── ShellInitializer.cs                                 #   壳视图初始化器
│   │   ├── ShellWindow.axaml                                   #   壳窗口 XAML
│   │   └── ShellWindow.axaml.cs                                #   壳窗口代码
│   └── ClassSchedule.UI.Shared/                                #  UI 共享层
│       ├── App.axaml                                           #   应用定义
│       ├── App.axaml.cs                                        #   应用类
│       ├── ApplicationLifetimeNotSupportedException.cs         #   生命周期不支持异常
│       ├── ClassSchedule.UI.Shared.csproj                      #   项目文件
│       ├── Constants.cs                                        #   UI 常量
│       ├── IServiceCollectionExtensions.cs                     #   DI 注册扩展
│       ├── IShellInitializer.cs                                #   壳视图初始化器接口
│       ├── UIOptions.cs                                        #   UI 选项
│       ├── ViewLocator.cs                                      #   ViewModel → View 定位器
│       ├── Models/                                             #   数据模型
│       │   ├── ThemeMode.cs                                    #    主题模式枚举
│       │   └── Toast.cs                                        #    Toast 提示条目
│       ├── ViewModels/                                         #   视图模型
│       │   ├── ShellViewModel.cs                               #   壳视图模型
│       │   └── ToastViewModel.cs                               #   Toast 提示视图模型
│       ├── Views/                                              #   视图
│       │   ├── ShellView.axaml                                 #   壳视图 XAML
│       │   ├── ShellView.axaml.cs                              #   壳视图代码
│       │   ├── ToastView.axaml                                 #   Toast 视图 XAML
│       │   └── ToastView.axaml.cs                              #   Toast 视图代码
├── tests/                                                      # 测试项目目录
│   ├── ClassSchedule.Domain.Tests/                             #  领域层单元测试
│   │   ├── ClassSchedule.Domain.Tests.csproj                   #   测试项目文件
│   │   ├── CourseTests.cs                                      #   课程实体单元测试
│   │   ├── DateOnlyExtensionsTests.cs                          #   DateOnly 扩展单元测试
│   │   ├── EnumExtensionsTests.cs                              #   枚举扩展单元测试
│   │   ├── ErrorCodeTests.cs                                   #   错误码单元测试
│   │   ├── OrdinalRangeTests.cs                                #   序数闭区间单元测试
│   │   ├── ResultTests.cs                                      #   结果类型单元测试
│   │   ├── TimetableCourseTests.cs                             #   课程表课程操作单元测试
│   │   ├── TimetableFragmentTests.cs                           #   课程表片段操作单元测试
│   │   ├── TimetablePeriodTests.cs                             #   课程表节次操作单元测试
│   │   └── TimetableTests.cs                                   #   课程表实体单元测试
│   ├── ClassSchedule.Infrastructure.Tests/                     #  基础设施层单元测试
│   │   ├── AssemblyInfo.cs                                     #   程序集级夹具与并行配置
│   │   ├── ClassSchedule.Infrastructure.Tests.csproj           #   测试项目文件
│   │   ├── FileLoggerOptionsTests.cs                           #   文件日志选项单元测试
│   │   ├── FileLoggerProviderTests.cs                          #   文件日志提供器单元测试
│   │   ├── FileSystemTests.cs                                  #   文件系统单元测试
│   │   ├── StringExtensionsTests.cs                            #   string 扩展单元测试
│   │   ├── TestConfigurationFactory.cs                         #   测试配置工厂
│   │   ├── TestEnvironmentFixture.cs                           #   测试环境夹具
│   │   └── UnhandledExceptionHelperTests.cs                    #   未处理异常辅助单元测试
│   └── ClassSchedule.UI.Shared.Tests/                          #  UI 共享层单元测试
│       ├── ApplicationLifetimeNotSupportedExceptionTests.cs    #   生命周期异常单元测试
│       ├── AssemblyInfo.cs                                     #   程序集级夹具与并行配置
│       ├── ClassSchedule.UI.Shared.Tests.csproj                #   测试项目文件
│       ├── CompositionRootTests.cs                             #   组合根解析单元测试
│       ├── FakeViewModel.cs                                    #   测试用视图模型
│       ├── TestApplication.cs                                  #   测试用无界面应用
│       ├── TestEnvironmentFixture.cs                           #   测试环境夹具
│       ├── ToastTests.cs                                       #   Toast 模型单元测试
│       ├── ToastViewModelTests.cs                              #   Toast 视图模型单元测试
│       ├── UIOptionsTests.cs                                   #   UI 选项单元测试
│       └── ViewLocatorTests.cs                                 #   视图定位器单元测试
├── .editorconfig                                               # 代码风格统一配置
├── .gitattributes                                              # Git 行尾归一化, diff 策略与二进制标记
├── .gitignore                                                  # 忽略规则
├── CHANGELOG.md                                                # 变更日志
├── ClassSchedule.slnx                                          # 解决方案文件
├── Directory.Build.props                                       # 全局构建属性
├── Directory.Packages.props                                    # 集中包版本管理
├── global.json                                                 # .NET 测试选项
├── LICENSE                                                     # MIT 许可证
└── README.md                                                   # 本文档
```

---

## 🤝 问题反馈与贡献

- 🐛 **反馈问题** — 遇到 Bug 或想提建议？前往 [Issues](https://github.com/xiting910/ClassSchedule/issues/new/choose) 提交（内置 Bug 报告 / 功能建议模板）
- 🚀 **贡献代码** — Fork 后提交 [PR](https://github.com/xiting910/ClassSchedule/pulls)，CI + CodeQL 自动守护
- 📖 **更新日志** — 版本演进记录见 [CHANGELOG.md](CHANGELOG.md)
- ⭐ **支持项目** — 觉得好用？点个 Star 支持一下～

---

## 📄 许可证

本项目采用 [MIT License](LICENSE)。
