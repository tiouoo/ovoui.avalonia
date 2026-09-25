using Avalonia.Styling;

namespace OvoUi.Common.Theme;

public static class ThemeVariants
{
    public static ThemeVariant Mirage { get; } = new("Mirage", ThemeVariant.Dark);
}

public enum Themes
{
    System,
    Light,
    Dark,
    Mirage
}