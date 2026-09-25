using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using OvoUi.Common.Extension;
using OvoUi.Common.Language;
using OvoUi.Common.Theme;

namespace OvoUi;

public partial class OvoUiTheme :  Styles
{
    private static readonly StyledProperty<Color> ThemeColorProperty =
        AvaloniaProperty.Register<OvoUiTheme, Color>(nameof(ThemeColor), Color.Parse("#1890ff"));

    private static readonly StyledProperty<Languages?> LanguageProperty =
        AvaloniaProperty.Register<OvoUiTheme, Languages?>(nameof(Language));

    private static readonly StyledProperty<ILang?> CustomLanguageProperty =
        AvaloniaProperty.Register<OvoUiTheme, ILang?>(nameof(CustomLanguage));

    public OvoUiTheme()
    {
        if (CustomLanguage != null)
            LangManager.SetLanguage(CustomLanguage);
        else if (Language != null)
            LangManager.SetLanguage(Language.Value);
        else
            LangManager.SetLanguage(Languages.zh_cn);

        ThemeManager.SetThemeColor(ThemeColor);

        this.GetObservable(ThemeColorProperty).Subscribe(ThemeManager.SetThemeColor);

        this.GetObservable(LanguageProperty).Subscribe(lang =>
        {
            if (lang.HasValue)
                LangManager.SetLanguage(lang.Value);
        });

        this.GetObservable(CustomLanguageProperty).Subscribe(customLang =>
        {
            if (customLang != null)
                LangManager.SetLanguage(customLang);
        });

        AvaloniaXamlLoader.Load(this);
    }

    public Color ThemeColor
    {
        get => GetValue(ThemeColorProperty);
        set => SetValue(ThemeColorProperty, value);
    }

    public Languages? Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    public ILang? CustomLanguage
    {
        get => GetValue(CustomLanguageProperty);
        set => SetValue(CustomLanguageProperty, value);
    }

    public void SetThemeColor(Color color)
    {
        ThemeColor = color;
    }

    public void SetThemeColor(string hexColor)
    {
        if (Color.TryParse(hexColor, out var color))
        {
            ThemeColor = color;
        }
    }

    public void SetLanguage(Languages lang)
    {
        Language = lang;
    }

    public void SetLanguage(ILang customLang)
    {
        CustomLanguage = customLang;
    }
}