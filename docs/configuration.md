# 进阶配置

本文介绍如何在 Code Behind 中设置语言与主题色、在运行时切换配置，以及添加自定义语言。

## 在 Code Behind 中设置

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
```

## 在运行时切换

```csharp
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

## 自定义语言

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

[返回 README](../README.md)
