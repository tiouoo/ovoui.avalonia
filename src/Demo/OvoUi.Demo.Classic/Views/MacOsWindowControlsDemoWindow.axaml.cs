using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OvoUi.Platform.MacOS;

namespace OvoUi.Demo.Classic.Views;

public partial class MacOsWindowControlsDemoWindow : Window
{
    public MacOsWindowControlsDemoWindow()
    {
        InitializeComponent();
        Opened += (_, _) => CaptureNativeDefaults();
    }

    private void UseSpacing(object? sender, RoutedEventArgs e)
    {
        var originX = Number(OriginX);
        var originY = Number(OriginY);
        var spacing = Number(Spacing);
        CloseX.Value = (decimal)originX;
        CloseY.Value = (decimal)originY;
        MinimizeX.Value = (decimal)(originX + spacing);
        MinimizeY.Value = (decimal)originY;
        ZoomX.Value = (decimal)(originX + spacing * 2);
        ZoomY.Value = (decimal)originY;
        ApplyCurrentOptions();
    }

    private void ApplyOptions(object? sender, RoutedEventArgs e) => ApplyCurrentOptions();

    private void RestoreDefaults(object? sender, RoutedEventArgs e)
    {
        CloseOverrideSize.IsChecked = false;
        MinimizeOverrideSize.IsChecked = false;
        ZoomOverrideSize.IsChecked = false;
        if (MacOsWindowControls.TryRestoreDefaults(this, out var error) &&
            MacOsWindowControls.TryGetDefaults(this, out var defaults, out error) &&
            defaults is not null)
        {
            PopulateForm(defaults);
            StatusText.Text = "Restored the native state captured when this window opened.";
            return;
        }

        StatusText.Text = error;
    }

    private void CaptureNativeDefaults()
    {
        if (MacOsWindowControls.TryGetDefaults(this, out var defaults, out var error) && defaults is not null)
        {
            PopulateForm(defaults);
            StatusText.Text = "Native defaults captured. Size remains unchanged until override is enabled.";
            return;
        }

        StatusText.Text = error;
    }

    private void PopulateForm(MacOsWindowControlsOptions options)
    {
        PopulateButton(options.Close, CloseX, CloseY, CloseWidth, CloseHeight, CloseOpacity, CloseVisible,
            CloseEnabled);
        PopulateButton(options.Minimize, MinimizeX, MinimizeY, MinimizeWidth, MinimizeHeight, MinimizeOpacity,
            MinimizeVisible, MinimizeEnabled);
        PopulateButton(options.Zoom, ZoomX, ZoomY, ZoomWidth, ZoomHeight, ZoomOpacity, ZoomVisible, ZoomEnabled);

        if (options.Close.Position is { } close && options.Minimize.Position is { } minimize)
        {
            OriginX.Value = (decimal)close.X;
            OriginY.Value = (decimal)close.Y;
            Spacing.Value = (decimal)(minimize.X - close.X);
        }
    }

    private static void PopulateButton(
        MacOsWindowButtonOptions options,
        NumericUpDown x,
        NumericUpDown y,
        NumericUpDown width,
        NumericUpDown height,
        NumericUpDown opacity,
        CheckBox visible,
        CheckBox enabled)
    {
        if (options.Position is { } position)
        {
            x.Value = (decimal)position.X;
            y.Value = (decimal)position.Y;
        }

        if (options.Size is { } size)
        {
            width.Value = (decimal)size.Width;
            height.Value = (decimal)size.Height;
        }

        opacity.Value = (decimal)options.Opacity;
        visible.IsChecked = options.IsVisible;
        enabled.IsChecked = options.IsEnabled;
    }

    private void ApplyCurrentOptions()
    {
        var options = new MacOsWindowControlsOptions
        {
            OriginX = Number(OriginX),
            OriginY = Number(OriginY),
            Spacing = Number(Spacing),
            Close = CreateButtonOptions(CloseX, CloseY, CloseWidth, CloseHeight, CloseOpacity, CloseVisible,
                CloseEnabled, CloseOverrideSize),
            Minimize = CreateButtonOptions(MinimizeX, MinimizeY, MinimizeWidth, MinimizeHeight, MinimizeOpacity,
                MinimizeVisible, MinimizeEnabled, MinimizeOverrideSize),
            Zoom = CreateButtonOptions(ZoomX, ZoomY, ZoomWidth, ZoomHeight, ZoomOpacity, ZoomVisible, ZoomEnabled,
                ZoomOverrideSize)
        };

        StatusText.Text = MacOsWindowControls.TryApply(this, options, out var error)
            ? "Applied to the native window."
            : error;
    }

    private static MacOsWindowButtonOptions CreateButtonOptions(
        NumericUpDown x,
        NumericUpDown y,
        NumericUpDown width,
        NumericUpDown height,
        NumericUpDown opacity,
        CheckBox visible,
        CheckBox enabled,
        CheckBox overrideSize) => new()
    {
        Position = new Point(Number(x), Number(y)),
        Size = overrideSize.IsChecked == true ? new Size(Number(width), Number(height)) : null,
        Opacity = Number(opacity),
        IsVisible = visible.IsChecked == true,
        IsEnabled = enabled.IsChecked == true
    };

    private static double Number(NumericUpDown input) => decimal.ToDouble(input.Value ?? 0);
}