<p align="center">
  <a href="https://ifdian.net/a/tiouo">
    <img src="assets/top.png" alt="赞助支持 OvoUi 持续开发">
  </a>
  <a href="https://github.com/tiouoo/ovoui.avalonia">
    <img src="assets/header.png" alt="OvoUi.Avalonia">
  </a>
</p>

<p align="start">
  <a href="https://github.com/tiouoo/ovoui.avalonia/actions/workflows/publish-nuget.yml"><img src="https://img.shields.io/github/actions/workflow/status/tiouoo/ovoui.avalonia/publish-nuget.yml?label=%E5%8F%91%E5%B8%83&logo=github&style=for-the-badge" alt="发布状态"></a>
  <a href="https://www.nuget.org/packages/OvoUi"><img src="https://img.shields.io/nuget/v/OvoUi?label=NuGet&logo=nuget&style=for-the-badge" alt="NuGet 版本"></a>
  <a href="https://www.nuget.org/packages/OvoUi"><img src="https://img.shields.io/nuget/dt/OvoUi?label=%E4%B8%8B%E8%BD%BD%E9%87%8F&logo=nuget&style=for-the-badge" alt="NuGet 下载量"></a>
  <a href="https://github.com/tiouoo/ovoui.avalonia/stargazers"><img src="https://img.shields.io/github/stars/tiouoo/ovoui.avalonia?label=%E6%98%9F%E6%A0%87&logo=github&logoColor=white&color=eac54f&style=for-the-badge" alt="星标"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/%E8%AE%B8%E5%8F%AF%E8%AF%81-Apache--2.0-D22128?logo=apache&logoColor=white&style=for-the-badge" alt="Apache-2.0 许可证"></a>
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white&style=for-the-badge" alt=".NET 10">
  <img src="https://img.shields.io/badge/Avalonia-12.1-B327F6?style=for-the-badge" alt="Avalonia 12.1">
</p>

---

## 在线演示

[OvoUi Browser Demo](https://ovoui.y6.nz)

## 安装

### 通过 NuGet 安装

```bash
dotnet add package OvoUi
```

其他功能可按需安装：

```bash
dotnet add package OvoUi.Fonts
dotnet add package OvoUi.Platform
dotnet add package OvoUi.AvaloniaEdit
dotnet add package OvoUi.AvaloniaEdit.Showcase
```

## 快速开始

### 配置 App.axaml

在应用程序中引入 OvoUi 主题：

```xml
<Application
    x:Class="YourApp.App"
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:ovo="https://github.com/tiouoo/ovoui.avalonia">
    <Application.Styles>
        <ovo:OvoUiTheme />
    </Application.Styles>
</Application>
```

### 设置语言与主题色（可选）

```xml
<ovo:OvoUiTheme Language="zh_cn" ThemeColor="#1890ff" />
```

或在 Code Behind 中设置：

```csharp
using OvoUi.Common.Language;
using OvoUi.Common.Theme;

public class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        // 设置默认语言为简体中文
        LangManager.SetLanguage(Languages.zh_cn);

        // 或设置为英文
        // LangManager.SetLanguage(Languages.en_us);

        // 设置主题色
        ThemeManager.SetThemeColor("#1890ff");

        base.OnFrameworkInitializationCompleted();
    }
}

// 在运行时切换语言
public void SetLanguage()
{
    LangManager.SetLanguage(Languages.en_us);
}

// 在运行时切换主题色
public void SetThemeColor()
{
    ThemeManager.SetThemeColor("#52c41a");
}
```

#### 自定义语言

在项目中创建一个新类并实现 `ILang` 接口：

```csharp
using OvoUi.Common.Language;

namespace YourApp.Languages;

public class LangJaJp : ILang
{
    public string Name => "Name";
    public string FileName => "File name";
    public string UpdateAt => "Updated at";
    public string Type => "Type";
    // 实现 ILang 中的其他成员
}
```

使用自定义语言：

```csharp
using OvoUi.Common.Language;
using YourApp.Languages;

LangManager.SetLanguage(new LangJaJp());
```

## 致谢

OvoUi 的开发受到了以下优秀项目的启发：

- [Semi.Avalonia](https://github.com/irihitech/Semi.Avalonia)
- [Ursa.Avalonia](https://github.com/irihitech/Ursa.Avalonia)
- [SukiUI](https://github.com/kikipoulet/SukiUI)

## 许可证

Copyright 2026 tiouo。

本项目采用 [Apache License 2.0](LICENSE) 许可证。
