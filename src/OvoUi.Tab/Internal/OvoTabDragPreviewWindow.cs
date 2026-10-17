using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace OvoUi.Tab.Internal;

internal sealed class OvoTabDragPreviewWindow : Window
{
    private readonly Border _root;
    private readonly TextBlock _glyph;
    private readonly ContentControl _header;
    private readonly PathIcon _icon;
    private readonly PathIcon _stateIcon;

    public OvoTabDragPreviewWindow()
    {
        WindowDecorations = Avalonia.Controls.WindowDecorations.None;
        ShowInTaskbar = false;
        ShowActivated = false;
        Topmost = true;
        CanResize = false;
        IsHitTestVisible = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        Height = 30;
        Background = Brushes.Transparent;
        TransparencyBackgroundFallback = Brushes.Transparent;

        _glyph = new TextBlock
        {
            FontSize = 16,
            Width = 18,
            Height = 18,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 1, 4, 0),
            IsVisible = false
        };
        _icon = new PathIcon
        {
            Width = 18, Height = 18, Margin = new Thickness(0, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center, IsVisible = false
        };
        _header = new ContentControl
        {
            FontSize = 14, Margin = new Thickness(2, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center
        };
        _stateIcon = new PathIcon
        {
            Width = 16, Height = 16, Margin = new Thickness(15, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center
        };

        var content = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto"),
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _glyph, _icon, _header, _stateIcon }
        };
        Grid.SetColumn(_glyph, 0);
        Grid.SetColumn(_icon, 0);
        Grid.SetColumn(_header, 1);
        Grid.SetColumn(_stateIcon, 2);

        _root = new Border
        {
            MinWidth = 135,
            Height = 30,
            Padding = new Thickness(8, 6),
            CornerRadius = new CornerRadius(10),
            BorderThickness = new Thickness(1),
            Child = content
        };
        Content = _root;
        ActualThemeVariantChanged += (_, _) => ApplyThemeColors();
        ApplyThemeColors();
        UpdateState(OvoTabDragState.Blocked);
    }

    public void UpdateTab(OvoTabItem tab)
    {
        // A control header already belongs to the source strip; the preview must not reparent it.
        _header.Content = tab.Header is Control ? tab.Title : tab.Header;
        _glyph.Text = tab.IconGlyph;
        _glyph.FontFamily = tab.IconFontFamily;
        _glyph.IsVisible = tab.HasGlyphIcon;
        _icon.Data = tab.Icon;
        _icon.IsVisible = tab.HasPathIcon;
    }

    public void UpdateState(OvoTabDragState state)
    {
        _stateIcon.Data = state switch
        {
            OvoTabDragState.Reorder => OvoTabIcons.Reorder,
            OvoTabDragState.Transfer => OvoTabIcons.Transfer,
            OvoTabDragState.Detach => OvoTabIcons.Detach,
            OvoTabDragState.CurrentWindow => OvoTabIcons.CurrentWindow,
            _ => OvoTabIcons.Blocked
        };
    }

    public void MoveTo(PixelPoint position) => Position = new PixelPoint(position.X + 12, position.Y + 12);

    private void ApplyThemeColors()
    {
        var dark = ActualThemeVariant == ThemeVariant.Dark;
        _root.Background = new SolidColorBrush(Color.Parse(dark ? "#1f2430" : "#ffffff"));
        _root.BorderBrush = new SolidColorBrush(Color.Parse(dark ? "#141821" : "#e7eaed"));
    }
}