using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using OvoUi.Common.Helpers;

namespace OvoUi.Controls;

[TemplatePart(PART_CloseButton, typeof(Button))]
[TemplatePart(PART_TitleArea, typeof(Panel))]
[TemplatePart(PART_NoButton, typeof(Button))]
[TemplatePart(PART_OKButton, typeof(Button))]
[TemplatePart(PART_CancelButton, typeof(Button))]
[TemplatePart(PART_YesButton, typeof(Button))]
public class MessageBoxWindow : Window
{
    public const string PART_CloseButton = "PART_CloseButton";
    public const string PART_TitleArea = "PART_TitleArea";
    public const string PART_YesButton = "PART_YesButton";
    public const string PART_NoButton = "PART_NoButton";
    public const string PART_OKButton = "PART_OKButton";
    public const string PART_CancelButton = "PART_CancelButton";

    public static readonly StyledProperty<MessageBoxIcon> MessageIconProperty =
        AvaloniaProperty.Register<MessageBoxWindow, MessageBoxIcon>(nameof(MessageIcon), MessageBoxIcon.None);

    private readonly MessageBoxButton _buttons;
    private Button? _closeButton;
    private Button? _okButton;
    private Button? _cancelButton;
    private Button? _yesButton;
    private Button? _noButton;
    private Panel? _titleArea;

    public MessageBoxWindow() : this(MessageBoxButton.OK)
    {
    }

    public MessageBoxWindow(MessageBoxButton buttons) => _buttons = buttons;

    protected override Type StyleKeyOverride => typeof(MessageBoxWindow);

    public MessageBoxIcon MessageIcon
    {
        get => GetValue(MessageIconProperty);
        set => SetValue(MessageIconProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Button.ClickEvent.RemoveHandler(OnButtonClick, _closeButton, _okButton, _cancelButton, _yesButton, _noButton);
        _titleArea?.RemoveHandler(PointerPressedEvent, OnTitleAreaPointerPressed);
        _closeButton = e.NameScope.Find<Button>(PART_CloseButton);
        _titleArea = e.NameScope.Find<Panel>(PART_TitleArea);
        _okButton = e.NameScope.Find<Button>(PART_OKButton);
        _cancelButton = e.NameScope.Find<Button>(PART_CancelButton);
        _yesButton = e.NameScope.Find<Button>(PART_YesButton);
        _noButton = e.NameScope.Find<Button>(PART_NoButton);
        Button.ClickEvent.AddHandler(OnButtonClick, _closeButton, _okButton, _cancelButton, _yesButton, _noButton);
        _titleArea?.AddHandler(PointerPressedEvent, OnTitleAreaPointerPressed, RoutingStrategies.Bubble);
        SetButtonVisibility();
    }

    private void OnTitleAreaPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void SetButtonVisibility()
    {
        SetVisible(_closeButton, _buttons != MessageBoxButton.YesNo);
        SetVisible(_okButton, _buttons is MessageBoxButton.OK or MessageBoxButton.OKCancel);
        SetVisible(_cancelButton, _buttons is MessageBoxButton.OKCancel or MessageBoxButton.YesNoCancel);
        SetVisible(_yesButton, _buttons is MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel);
        SetVisible(_noButton, _buttons is MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel);
    }

    private static void SetVisible(Control? control, bool visible)
    {
        if (control is not null) control.IsVisible = visible;
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e) => Close(sender switch
    {
        _ when sender == _okButton => MessageBoxResult.OK,
        _ when sender == _yesButton => MessageBoxResult.Yes,
        _ when sender == _noButton => MessageBoxResult.No,
        _ => _buttons == MessageBoxButton.OK ? MessageBoxResult.OK : MessageBoxResult.Cancel
    });

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.Key == Key.Escape && _buttons != MessageBoxButton.YesNo)
            Close(_buttons == MessageBoxButton.OK ? MessageBoxResult.OK : MessageBoxResult.Cancel);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var button = _buttons switch
        {
            MessageBoxButton.OK or MessageBoxButton.OKCancel => _okButton,
            _ => _yesButton
        };
        if (button is not null) button.SetValue(Button.IsDefaultProperty, true);
        button?.Focus();
    }
}