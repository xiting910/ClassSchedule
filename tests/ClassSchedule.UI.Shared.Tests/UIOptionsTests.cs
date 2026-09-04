using ClassSchedule.Infrastructure;
using ClassSchedule.UI.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json.Nodes;

namespace ClassSchedule.UI.Shared.Tests;

/// <summary>
/// <see cref="UIOptions"/> 的单元测试
/// </summary>
public sealed class UIOptionsTests
{
    /// <summary>
    /// UI 设置文件的完整路径
    /// </summary>
    private static string UISettingsFilePath => Path.Combine(
        FileSystem.Settings.FullName, $"UISettings{FileSystem.JsonFileSuffix}"
    );

    /// <summary>
    /// 创建带指定键值的 UI 选项
    /// </summary>
    /// <param name="values">属性名与值键值对</param>
    /// <returns>带指定键值的 UI 选项</returns>
    private static UIOptions Create(params IEnumerable<(string Key, string? Value)> values)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(kv => $"{nameof(UIOptions)}:{kv.Key}", kv => kv.Value))
            .Build();
        return new(NullLogger<UIOptions>.Instance, config);
    }

    /// <summary>
    /// 验证无配置节时使用默认值: 主题跟随系统, 最大条数 2, 时长 5 秒
    /// </summary>
    [Fact]
    public void Ctor_无配置节_使用默认值()
    {
        var options = Create();

        Assert.Equal(ThemeMode.System, options.Theme);
        Assert.Equal(2, options.MaxToastCount);
        Assert.Equal(5.0, options.ToastDurationSeconds);
    }

    /// <summary>
    /// 验证配置有效时按配置解析
    /// </summary>
    [Fact]
    public void Ctor_配置有效_按配置解析()
    {
        var options = Create(
            (nameof(UIOptions.Theme), nameof(ThemeMode.Dark)),
            (nameof(UIOptions.MaxToastCount), "4"),
            (nameof(UIOptions.ToastDurationSeconds), "3.5")
        );

        Assert.Equal(ThemeMode.Dark, options.Theme);
        Assert.Equal(4, options.MaxToastCount);
        Assert.Equal(3.5, options.ToastDurationSeconds);
    }

    /// <summary>
    /// 验证主题枚举无效时回退到跟随系统
    /// </summary>
    [Fact]
    public void Ctor_主题枚举无效_回退到System()
    {
        var options = Create((nameof(UIOptions.Theme), "NotATheme"));

        Assert.Equal(ThemeMode.System, options.Theme);
    }

    /// <summary>
    /// 验证最大条数为非法文本时回退到默认值 2
    /// </summary>
    [Fact]
    public void Ctor_条数非法_回退到默认值()
    {
        var options = Create((nameof(UIOptions.MaxToastCount), "not-a-number"));

        Assert.Equal(2, options.MaxToastCount);
    }

    /// <summary>
    /// 验证最大条数超上限时钳制到 5
    /// </summary>
    [Fact]
    public void Ctor_条数超上限_钳制到5()
    {
        var options = Create((nameof(UIOptions.MaxToastCount), "99"));

        Assert.Equal(5, options.MaxToastCount);
    }

    /// <summary>
    /// 验证最大条数为负数时钳制到 0
    /// </summary>
    [Fact]
    public void Ctor_条数为负数_钳制到0()
    {
        var options = Create((nameof(UIOptions.MaxToastCount), "-3"));

        Assert.Equal(0, options.MaxToastCount);
    }

    /// <summary>
    /// 验证时长为非法文本时回退到默认值 5 秒
    /// </summary>
    [Fact]
    public void Ctor_时长非法_回退到默认值()
    {
        var options = Create((nameof(UIOptions.ToastDurationSeconds), "abc"));

        Assert.Equal(5.0, options.ToastDurationSeconds);
    }

    /// <summary>
    /// 验证时长超上限时钳制到 10 秒
    /// </summary>
    [Fact]
    public void Ctor_时长超上限_钳制到10()
    {
        var options = Create((nameof(UIOptions.ToastDurationSeconds), "99"));

        Assert.Equal(10.0, options.ToastDurationSeconds);
    }

    /// <summary>
    /// 验证时长为负数时钳制到 0
    /// </summary>
    [Fact]
    public void Ctor_时长为负数_钳制到0()
    {
        var options = Create((nameof(UIOptions.ToastDurationSeconds), "-5"));

        Assert.Equal(0, options.ToastDurationSeconds);
    }

    /// <summary>
    /// 验证属性变化时保存到设置文件, 且文件可被重新加载读回
    /// </summary>
    [Fact]
    public void Setter_修改属性_保存文件并可重新加载()
    {
        var options = Create();
        options.Theme = ThemeMode.Light;
        options.MaxToastCount = 3;
        options.ToastDurationSeconds = 7.5;

        Assert.True(File.Exists(UISettingsFilePath));
        var node = JsonNode.Parse(File.ReadAllText(UISettingsFilePath))![nameof(UIOptions)]!;
        Assert.Equal(nameof(ThemeMode.Light), node[nameof(UIOptions.Theme)]!.GetValue<string>());
        Assert.Equal(3, node[nameof(UIOptions.MaxToastCount)]!.GetValue<int>());
        Assert.Equal(7.5, node[nameof(UIOptions.ToastDurationSeconds)]!.GetValue<double>());

        var config = new ConfigurationBuilder().AddJsonFile(UISettingsFilePath).Build();
        var loaded = new UIOptions(NullLogger<UIOptions>.Instance, config);
        Assert.Equal(ThemeMode.Light, loaded.Theme);
        Assert.Equal(3, loaded.MaxToastCount);
        Assert.Equal(7.5, loaded.ToastDurationSeconds);
    }

    /// <summary>
    /// 验证属性被赋予相同值时不会重复写入设置文件
    /// </summary>
    [Fact]
    public void Setter_相同值_不重写设置文件()
    {
        var options = Create();
        options.Theme = ThemeMode.Light;
        options.MaxToastCount = 3;
        Assert.True(File.Exists(UISettingsFilePath));

        const string Sentinel = "SENTINEL-NOT-A-JSON";
        File.WriteAllText(UISettingsFilePath, Sentinel);

        options.Theme = ThemeMode.Light;
        options.MaxToastCount = 3;
        Assert.Equal(Sentinel, File.ReadAllText(UISettingsFilePath));

        options.Theme = ThemeMode.Dark;
        var root = JsonNode.Parse(File.ReadAllText(UISettingsFilePath));
        Assert.Equal(
            nameof(ThemeMode.Dark), root![nameof(UIOptions)]![nameof(UIOptions.Theme)]!.GetValue<string>()
        );
    }
}
