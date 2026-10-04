using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using OvoUi.Common.Enums;

namespace OvoUi.Controls;

public class DialogOptions
{
    internal static DialogOptions Default { get; } = new DialogOptions();

    /// <summary>
    /// The Startup Location of DialogWindow. Default is <see cref="WindowStartupLocation.CenterOwner"/>
    /// </summary>
    public WindowStartupLocation StartupLocation { get; set; } = WindowStartupLocation.CenterOwner;

    /// <summary>
    /// The Position of DialogWindow startup location if <see cref="StartupLocation"/> is <see cref="WindowStartupLocation.Manual"/>
    /// </summary>
    public PixelPoint? Position { get; set; }

    /// <summary>
    /// Title of DialogWindow, Default is null
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// DialogWindow's Mode, Default is <see cref="DialogMode.None"/>
    /// </summary>
    public DialogMode Mode { get; set; } = DialogMode.None;

    public DialogButton Buttons { get; set; } = DialogButton.OKCancel;

    public bool? IsCloseButtonVisible { get; set; } = true;

    public bool ShowInTaskBar { get; set; } = true;

    public bool CanDragMove { get; set; } = true;

    public bool CanResize { get; set; }
    public string? StyleClass { get; set; }

    /// <summary>
    /// Visibility of the horizontal scrollbar inside the dialog content area. Default is <see cref="ScrollBarVisibility.Auto"/>.
    /// </summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; } = ScrollBarVisibility.Auto;

    /// <summary>
    /// Visibility of the vertical scrollbar inside the dialog content area. Default is <see cref="ScrollBarVisibility.Auto"/>.
    /// </summary>
    public ScrollBarVisibility VerticalScrollBarVisibility { get; set; } = ScrollBarVisibility.Auto;

    public string? OverrideOkButtonText { get; set; }
    public string? OverrideCancelButtonText { get; set; }
    public string? OverrideYesButtonText { get; set; }
    public string? OverrideNoButtonText { get; set; }

    public double DialogWindowMinWidth { get; set; } = Double.NaN;
    public double DialogWindowMinHeight { get; set; } = Double.NaN;
    public double DialogWindowMaxWidth { get; set; } = double.PositiveInfinity;
    public double DialogWindowMaxHeight { get; set; } = double.PositiveInfinity;
    public double DialogWindowWidth { get; set; } = Double.NaN;
    public double DialogWindowHeight { get; set; } = Double.NaN;
}