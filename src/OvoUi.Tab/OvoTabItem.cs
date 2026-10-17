using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;

namespace OvoUi.Tab;

/// <summary>
/// A host-independent tab model. It can be transferred between hosts without retaining a window reference.
/// </summary>
public sealed class OvoTabItem : INotifyPropertyChanged
{
    private object? _content;
    private object? _header;
    private StreamGeometry? _icon;
    private FontFamily? _iconFontFamily;
    private string? _iconGlyph;
    private double _iconHeight = 16;
    private Thickness _iconMargin = new(0, 0, 4, 0);
    private double _iconWidth = 16;
    private bool _isCloseable = true;
    private bool _isDragging;
    private bool _isIconVisible = true;
    private bool _isClosing;
    private double _maximumWidth = double.PositiveInfinity;
    private double _minimumWidth = 150;
    private string _title;

    public OvoTabItem(string title, object? content = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        _title = title;
        _header = title;
        _content = content;
    }

    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string Title
    {
        get => _title;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            if (!SetField(ref _title, value)) return;
            if (Header is string) Header = value;
        }
    }

    public object? Header
    {
        get => _header;
        set => SetField(ref _header, value);
    }

    public object? Content
    {
        get => _content;
        set => SetField(ref _content, value);
    }

    public StreamGeometry? Icon
    {
        get => _icon;
        set
        {
            if (!SetField(ref _icon, value)) return;
            OnPropertyChanged(nameof(HasPathIcon));
            OnPropertyChanged(nameof(HasIcon));
        }
    }

    public string? IconGlyph
    {
        get => _iconGlyph;
        set
        {
            if (!SetField(ref _iconGlyph, value)) return;
            OnPropertyChanged(nameof(HasGlyphIcon));
            OnPropertyChanged(nameof(HasPathIcon));
            OnPropertyChanged(nameof(HasIcon));
        }
    }

    public FontFamily? IconFontFamily
    {
        get => _iconFontFamily;
        set => SetField(ref _iconFontFamily, value);
    }

    public double IconWidth
    {
        get => _iconWidth;
        set => SetField(ref _iconWidth, Math.Max(0, value));
    }

    public double IconHeight
    {
        get => _iconHeight;
        set => SetField(ref _iconHeight, Math.Max(0, value));
    }

    public Thickness IconMargin
    {
        get => _iconMargin;
        set => SetField(ref _iconMargin, value);
    }

    public bool IsCloseable
    {
        get => _isCloseable;
        set => SetField(ref _isCloseable, value);
    }

    public bool IsIconVisible
    {
        get => _isIconVisible;
        set
        {
            if (!SetField(ref _isIconVisible, value)) return;
            OnPropertyChanged(nameof(HasIcon));
            OnPropertyChanged(nameof(HasGlyphIcon));
            OnPropertyChanged(nameof(HasPathIcon));
        }
    }

    public double MinimumWidth
    {
        get => _minimumWidth;
        set => SetField(ref _minimumWidth, Math.Max(0, value));
    }

    public double MaximumWidth
    {
        get => _maximumWidth;
        set => SetField(ref _maximumWidth, Math.Max(0, value));
    }

    public object? Tag { get; set; }

    public bool IsDragging
    {
        get => _isDragging;
        internal set => SetField(ref _isDragging, value);
    }

    public bool HasGlyphIcon => IsIconVisible && !string.IsNullOrEmpty(IconGlyph);

    public bool HasPathIcon => IsIconVisible && !HasGlyphIcon && Icon is not null;

    public bool HasIcon => HasGlyphIcon || HasPathIcon;

    internal bool TryBeginClose()
    {
        if (_isClosing) return false;
        _isClosing = true;
        return true;
    }

    internal void EndClose() => _isClosing = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}