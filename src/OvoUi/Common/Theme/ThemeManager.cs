using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace OvoUi.Common.Theme;

public class ThemeManager : INotifyPropertyChanged
{
    private ThemeManager()
    {
    }

    
    public static ThemeManager Instance { get; } = new();

    public Color CurrentColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentColor)));
            ThemeHelper.SetThemeColor(value);
        }
    }

    public static Color ThemeColor => Instance.CurrentColor;

    public event PropertyChangedEventHandler? PropertyChanged;

    public static void SetThemeColor(Color color)
    {
        Instance.CurrentColor = color;
    }

    public static void SetThemeColor(string hexColor)
    {
        if (Color.TryParse(hexColor, out var color))
        {
            Instance.CurrentColor = color;
        }
    }
    
    public static void ToggleTheme(Themes theme)
    {
        Application.Current.RequestedThemeVariant = theme switch
        {
            Themes.Light => ThemeVariant.Light,
            Themes.Dark => ThemeVariant.Dark,
            Themes.System => ThemeVariant.Default,
            _ => Application.Current.RequestedThemeVariant
        };
    }
}