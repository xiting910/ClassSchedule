# Changelog

本文件记录了项目的所有重要变更。每个版本的变更都应在发布时记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/).

---

## [Unreleased]

### Added
- 添加应用壳导航栈, ShellViewModel 支持按页面视图模型类型入栈与出栈, 每个入栈页在独立的服务范围里解析, 出栈时释放
- 添加 IPageViewModel 与 IPageViewModel<TArg> 页面视图模型接口, 由页面自行实现载入逻辑并返回结果
- 添加物理返回键处理, 安卓端优先弹栈, 栈空时回退到课表 Tab, 两者都不满足则交还系统
- 添加应用壳的底部 TabControl, 承载课表、课程与设置三个占位页
- 添加壳视图模型的单元测试, 覆盖入栈成功与失败、异常处理、服务范围释放、返回键弹栈与 Tab 回退
- 添加 UI 资源 CardCornerRadius 与 CardPadding, 统一卡片圆角与内边距
- 添加 UI 选项 CurrentTimetableId 与 ShowWeekends, 分别记录当前课程表 ID 与是否显示周末, 并随设置文件持久化
- 添加 VS Code 工作区设置, 指定 C# Dev Kit 默认加载的解决方案文件
- 添加 Timetable.ChangeAllPeriodDurations, 保持各节次定义的开始时间不变并统一持续时间, 放不下时不做任何修改
- 添加 Timetable.ChangeAllPeriodDurations 的领域层单元测试
- 添加 Timetable.ChangeFragmentWeeks 的领域层单元测试, 覆盖更改后的周次超出总周数返回 WeekNotFound 且不修改原有周次
- 添加 EF Core 持久化层, 由 AppDbContext 承载课程表聚合, 并按实体拆分配置类
- 添加课程片段的节次区间到 JSON 单列的转换器
- 添加课程片段的周次区间列表到 JSON 单列的转换器与比较器
- 添加实体物化拦截器, 课程表实体取回后自动按序号升序排列课程节次定义
- 添加 ITimetableRepository 课程表仓储接口与实现, 提供列表, 取回, 新增, 删除与保存
- 添加 IDatabaseInitializer 数据库初始化器接口与实现, 启动时应用迁移
- 添加 AppDbContext 的设计时工厂, 供 EF Core 命令行工具生成迁移
- 添加 InitialCreate 首次建表迁移与模型快照
- 添加 TimetableSummary 课表摘要模型, 供列表展示
- 添加基础设施层持久化单元测试, 覆盖仓储增删改查、课程片段 JSON 列往返与实体物化拦截器
- 支持 OrdinalRange 的 JSON 反序列化, 为周次集合的 JSON 列持久化做准备
- 启用全局 TreatWarningsAsErrors, 所有编译器警告视为错误
- 添加 WeekRange 周段模型与 Weekday 星期枚举领域模型, 周段支持包含与交集判断
- 添加 OrdinalRange 序数闭区间模型, 支持包含与交集判断, 并支持区间列表排序、相邻合并与重叠判定的归一化
- 添加 Result 结果类型层次结构, 由无值成功、带值成功与失败三个封闭子类型区分, 非法状态不可表示
- 添加 ErrorCode 错误码枚举, 成员按需添加, 由 Description 特性承载中文描述
- 添加 EnumExtensions.GetDescription, 反射读取枚举成员的 Description 特性, 缺失时回退成员名
- 添加 Timetable 课程表聚合根, 按分部类拆分为节次, 课程与片段三组操作
- 添加 Course、Fragment 与 PeriodDefinition 实体, 均为按身份比较的封闭类
- 添加 SemesterDay 学期日模型, 由学期周次与星期几两个字段组成
- 添加 Timetable.TryGetSemesterDay, 按首周周一与总周数换算日期对应的学期周次与星期几
- 添加 DateOnlyExtensions.GetMonday 与 DayOfWeekExtensions.ToWeekday 扩展成员
- 扩展 ErrorCode 错误码枚举, 补充课程表, 周次, 课程, 片段与节次相关失败码
- 添加课程表聚合的领域层单元测试, 覆盖节次归并, 占用校验与片段重叠判定
- 添加 Timetable.TryGetSemesterDay 的领域层单元测试, 覆盖学期上下边界与跨年周号连续性
- 添加 OrdinalRangeTests 领域层单元测试, 覆盖序数闭区间归一化的排序、相邻合并与重叠拒绝场景
- 添加 Result、ErrorCode 与 EnumExtensions 的领域层单元测试, 覆盖结果层次结构、错误码描述完备性与枚举描述回退
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
- 数据库初始化器与课程表仓储的日志文案改为英文, 占位符名称、事件名与日志级别均未变
- 课程表仓储的日志事件 ID 整体后移一位 (0-4 变为 1-5), 日志文本、事件名与级别均未变
- 统一 AndroidManifest.xml 的缩进为 2 个空格, 与 .editorconfig 的 XML 缩进规则保持一致
- 壳视图模型改为构造函数注入服务范围工厂、日志记录器与 Toast 视图模型
- 壳视图更名职责为承载页面栈与全局提示, 并挂接加载与卸载钩子管理返回键, 全局提示始终位于最上层
- Toast 视图的卡片圆角与内边距改用应用级资源, 并在 XAML 中统一属性书写顺序
- 统一源文件编码为 UTF-8 无 BOM, 并统一 MainActivity 的 Activity 特性参数顺序
- Toast 提示适配安卓端: 显示位置由右下角改为顶部居中, 入场动画改为从上方滑入
- 安卓端壳视图优先通过 IActivityApplicationLifetime.MainViewFactory 创建, 兼顾单视图生命周期
- 应用启动时在组合根内应用数据库迁移, 由 IDatabaseInitializer 在服务容器返回前完成初始化
- AddInfrastructure 补齐持久化装配, 注册 AppDbContext、数据库初始化器与课程表仓储
- 基础设施层测试改用真实 SQLite 数据库, 由测试夹具复用 AddInfrastructure 构建服务容器并初始化数据库
- TimeProvider.System 的注册由 AddDomain 迁回 AddInfrastructure, Domain 层不再引用依赖注入抽象包
- TimeProvider.System 的注册由 AddInfrastructure 迁移到 AddDomain
- 文件写入不再静默吞异常, 改为显式处理并外抛 (SafeWriteToFile 更名为 WriteToFile)
- 优化文件日志轮转与写入: 空文件不轮转, UTF-8 无 BOM 编码并显式 Flush
- 未处理异常日志移入应用数据目录, 目录不可用时回退到程序基目录
- 为 UI 选项与文件日志选项添加结构化日志记录
- 升级 Avalonia 至 12.1.2 并简化 Dependabot 分组配置
- 服务容器装配收拢到 UI 共享层的 App.CreateServices, 平台入口只需传入各自的壳视图初始化器类型
- 服务容器启用作用域与构建期校验 (ValidateScopes 与 ValidateOnBuild), 注册缺失或生命周期错配在启动时即失败
- 依赖注入与日志包引用由桌面端、安卓端与 UI 共享层测试项目移入 UI 共享层, 由后者统一传递

### Removed
- 移除测试用视图模型 FakeViewModel, 由 FakePageViewModel 取代
- 移除 Microsoft.EntityFrameworkCore.InMemory 包引用, 测试不再依赖内存数据库
- 移除 Domain 层的 DI 注册扩展 AddDomain 及其依赖注入抽象包引用, 使领域层保持零依赖
- 移除桌面端、安卓端与 UI 共享层测试的组合根中对 AddDomain 的调用
- 移除 UI 共享层测试的 CompositionRootTests 组合根解析测试类, 其校验职责已由 App.CreateServices 的构建期校验承担
- 移除 WeekRange 周段模型, 由 OrdinalRange 取代, 使周次与节次共用同一个序数闭区间类型
- 移除 WeekRangeTests 单元测试类, 由 OrdinalRangeTests 覆盖序数闭区间及其归一化
- 移除 Fragment.EnsureWeeksSorted, 周次集合改由 JSON 列持久化, 不再存在子表查询后的乱序场景
- 移除 Toast 悬停暂停倒计时, 为安卓端适配, 一并移除 IsPaused 属性、指针进出事件处理器及其单元测试

### Fixed
- 修复文件日志丢失异常信息的问题, 异常文本现在会写入日志行

[Unreleased]: https://github.com/xiting910/ClassSchedule/commits/main
