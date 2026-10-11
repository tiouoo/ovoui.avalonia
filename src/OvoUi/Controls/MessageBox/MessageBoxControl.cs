using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using OvoUi.Common.Helpers;

namespace OvoUi.Controls;

[TemplatePart(PART_NoButton, typeof(Button))]
[TemplatePart(PART_OKButton, typeof(Button))]
[TemplatePart(PART_CancelButton, typeof(Button))]
[TemplatePart(PART_YesButton, typeof(Button))]
public class MessageBoxControl : DialogControlBase
{
    public const string PART_YesButton = "PART_YesButton";
    public const string PART_NoButton = "PART_NoButton";
    public const string PART_OKButton = "PART_OKButton";
    public const string PART_CancelButton = "PART_CancelButton";

    public static readonly StyledProperty<MessageBoxIcon> MessageIconProperty = AvaloniaProperty.Register<MessageBoxControl, MessageBoxIcon>(nameof(MessageIcon), MessageBoxIcon.None);
    public static readonly StyledProperty<MessageBoxButton> ButtonsProperty = AvaloniaProperty.Register<MessageBoxControl, MessageBoxButton>(nameof(Buttons));
    public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<MessageBoxControl, string?>(nameof(Title));

    private Button? _okButton;
    private Button? _cancelButton;
    private Button? _yesButton;
    private Button? _noButton;

    static MessageBoxControl() => ButtonsProperty.Changed.AddClassHandler<MessageBoxControl>((control, _) => control.SetButtonVisibility());

    public MessageBoxIcon MessageIcon { get => GetValue(MessageIconProperty); set => SetValue(MessageIconProperty, value); }
    public MessageBoxButton Buttons { get => GetValue(ButtonsProperty); set => SetValue(ButtonsProperty, value); }
    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Button.ClickEvent.RemoveHandler(OnButtonClick, _okButton, _cancelButton, _yesButton, _noButton);
        _okButton = e.NameScope.Find<Button>(PART_OKButton);
        _cancelButton = e.NameScope.Find<Button>(PART_CancelButton);
        _yesButton = e.NameScope.Find<Button>(PART_YesButton);
        _noButton = e.NameScope.Find<Button>(PART_NoButton);
        Button.ClickEvent.AddHandler(OnButtonClick, _okButton, _cancelButton, _yesButton, _noButton);
        SetButtonVisibility();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        GetDefaultButton()?.Focus();
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e) => OnElementClosing(this, sender switch
    {
        _ when sender == _okButton => MessageBoxResult.OK,
        _ when sender == _cancelButton => MessageBoxResult.Cancel,
        _ when sender == _yesButton => MessageBoxResult.Yes,
        _ when sender == _noButton => MessageBoxResult.No,
        _ => MessageBoxResult.None
    });

    private void SetButtonVisibility()
    {
        SetVisible(_closeButton, Buttons != MessageBoxButton.YesNo);
        SetVisible(_okButton, Buttons is MessageBoxButton.OK or MessageBoxButton.OKCancel);
        SetVisible(_cancelButton, Buttons is MessageBoxButton.OKCancel or MessageBoxButton.YesNoCancel);
        SetVisible(_yesButton, Buttons is MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel);
        SetVisible(_noButton, Buttons is MessageBoxButton.YesNo or MessageBoxButton.YesNoCancel);
        if (GetDefaultButton() is { } button) button.SetValue(Button.IsDefaultProperty, true);
    }

    private Button? GetDefaultButton() => Buttons switch
    {
        MessageBoxButton.OK => _okButton,
        MessageBoxButton.OKCancel => _okButton,
        MessageBoxButton.YesNo => _yesButton,
        MessageBoxButton.YesNoCancel => _yesButton,
        _ => null
    };

    private static void SetVisible(Control? control, bool visible)
    {
        if (control is not null) control.IsVisible = visible;
    }

    public override void Close() => OnElementClosing(this, Buttons switch
    {
        MessageBoxButton.OK => MessageBoxResult.OK,
        MessageBoxButton.YesNo => MessageBoxResult.No,
        _ => MessageBoxResult.Cancel
    });
}
