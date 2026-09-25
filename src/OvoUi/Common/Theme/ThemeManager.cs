using System.ComponentModel;
using Avalonia.Media;

namespace OvoUi.Common.Theme;

/// <summary>
/// 主题管理器，提供简单的API来设置主题色
/// </summary>
public class ThemeManager : INotifyPropertyChanged
{
    private Color _currentColor;

    private ThemeManager()
    {
    }

    public static ThemeManager Instance { get; } = new();

    /// <summary>
    /// 当前主题色
    /// </summary>
    public Color CurrentColor
    {
        get => _currentColor;
        set
        {
            if (_currentColor == value) return;
            _currentColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentColor)));
            ThemeHelper.SetThemeColor(value);
        }
    }

    public static Color ThemeColor => Instance.CurrentColor;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 设置主题色
    /// </summary>
    /// <param name="color">主题色</param>
    public static void SetThemeColor(Color color)
    {
        Instance.CurrentColor = color;
    }

    /// <summary>
    /// 设置主题色
    /// </summary>
    /// <param name="hexColor">十六进制颜色值，例如 "#1890ff"</param>
    public static void SetThemeColor(string hexColor)
    {
        if (Color.TryParse(hexColor, out var color))
        {
            Instance.CurrentColor = color;
        }
    }
}