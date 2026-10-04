using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using OvoUi.Common;
using OvoUi.Common.Classes;
using OvoUi.Common.Enums;
using OvoUi.Common.Helpers;
using OvoUi.Common.Interfaces;

namespace OvoUi.Controls;

[TemplatePart(PART_CloseButton, typeof(Button))]
public abstract class DrawerControlBase : OverlayFeedbackElement
{
    public const string PART_CloseButton = "PART_CloseButton";

    public static readonly StyledProperty<Position> PositionProperty =
        AvaloniaProperty.Register<DrawerControlBase, Position>(
            nameof(Position), Position.Right);

    public static readonly StyledProperty<bool> CanResizeProperty = AvaloniaProperty.Register<DrawerControlBase, bool>(
        nameof(CanResize));

    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<DrawerControlBase, bool>(
        nameof(IsOpen));

    protected internal Button? _closeButton;

    public bool CanResize
    {
        get => GetValue(CanResizeProperty);
        set => SetValue(CanResizeProperty, value);
    }

    public Position Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    internal bool? IsCloseButtonVisible { get; set; }

    protected internal bool CanLightDismiss { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Button.ClickEvent.RemoveHandler(OnCloseButtonClick, _closeButton);
        _closeButton = e.NameScope.Find<Button>(PART_CloseButton);
        Button.ClickEvent.AddHandler(OnCloseButtonClick, _closeButton);
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    public override void Close()
    {
        if (DataContext is IDialogContext context)
            context.Close();
        else
            RaiseEvent(new ResultEventArgs(ClosedEvent, null));
    }
}