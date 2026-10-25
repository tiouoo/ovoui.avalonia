using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using OvoUi.Lang;

namespace OvoUi.Demo.Models;

[LangManager("Resources/Lang/Strings.json", DefaultCulture = "en-US")]
[LangResource("zh-CN", "Resources/Lang/Custom/简体中文.json")]
public partial class DemoLanguage;

[LangManager("Resources/Lang/Labels.resx", DefaultCulture = "en-US")]
public partial class DemoResxLanguage;

public partial class LangDemoViewModel : ObservableObject
{
    public IList<ILangManager> Managers { get; } = [DemoLanguage.Instance, DemoResxLanguage.Instance];

    public IList<LangCulture> Cultures { get; } =
    [
        new() { CultureName = "en-US", DisplayName = "English", ShortDisplayName = "EN" },
        new() { CultureName = "zh-CN", DisplayName = "简体中文(自定义路径)", ShortDisplayName = "中文" },
        new() { CultureName = "ja-JP", DisplayName = "日本語", ShortDisplayName = "日本語" },
        new() { CultureName = "fr-FR", DisplayName = "Français", ShortDisplayName = "FR" }
    ];

    public IObservable<string?> Greeting => DemoLanguage.Instance.Greeting;
    public LangFormatObservable FormattedText { get; }

    [ObservableProperty] private decimal _pageNumber = 1;
    [ObservableProperty] private decimal _amount = 1234.5m;
    [ObservableProperty] private string _customGreeting = "✦ My custom translation / 我的自定义翻译";
    [ObservableProperty] private string _translationList = "";

    public LangDemoViewModel()
    {
        FormattedText = DemoLanguage.Keys.Page_Template.CreateFormat()
            .Arg(this, nameof(PageNumber), x => x.PageNumber)
            .Arg(this, nameof(Amount), x => x.Amount).Build();
        RefreshTranslations();
    }

    public void ApplyOverride()
    {
        var manager = DemoLanguage.Instance;
        manager.AddResources(manager.CurrentCulture, new Dictionary<string, string> { ["Greeting"] = CustomGreeting });
        manager.UpdateCulture(manager.CurrentCulture);
        RefreshTranslations();
    }

    public void ClearOverrides()
    {
        var manager = DemoLanguage.Instance;
        manager.ClearRuntimeResources();
        manager.UpdateCulture(manager.CurrentCulture);
        RefreshTranslations();
    }

    public void RefreshTranslations() => TranslationList = string.Join(Environment.NewLine,
        DemoLanguage.Instance.GetTranslations(DemoLanguage.Instance.Greeting)
            .OrderBy(p => p.Key.Name, StringComparer.Ordinal)
            .Select(p => $"{(p.Key.Equals(CultureInfo.InvariantCulture) ? "default" : p.Key.Name)}: {p.Value}"));
}