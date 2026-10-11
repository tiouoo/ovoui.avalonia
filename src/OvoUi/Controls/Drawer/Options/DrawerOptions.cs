using System.Windows.Input;
using Avalonia.Controls.Primitives;
using OvoUi.Common;
using OvoUi.Common.Enums;

namespace OvoUi.Controls.Options;

public class DrawerOptions
{
    internal static DrawerOptions Default => new();
    public Position Position { get; set; } = Position.Right;
    public bool CanLightDismiss { get; set; } = true;
    public bool? IsCloseButtonVisible { get; set; } = true;
    public double? MinWidth { get; set; }
    public double? MinHeight { get; set; }
    public double? MaxWidth { get; set; }
    public double? MaxHeight { get; set; }
    public DialogButton Buttons { get; set; } = DialogButton.OKCancel;
    public string? Title { get; set; }
    public ICommand? TitleCommand { get; set; }
    public int? TopLevelHashCode { get; set; }
    public bool CanResize { get; set; }
    public string? StyleClass { get; set; }
    public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; } = ScrollBarVisibility.Auto;
    public ScrollBarVisibility VerticalScrollBarVisibility { get; set; } = ScrollBarVisibility.Auto;
}
