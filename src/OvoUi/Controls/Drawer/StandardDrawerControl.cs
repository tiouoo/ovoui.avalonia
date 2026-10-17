using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using OvoUi.Common.Classes;
using OvoUi.Common.Enums;
using OvoUi.Common.Helpers;
using OvoUi.Common.Interfaces;

namespace OvoUi.Controls;

[TemplatePart(PART_YesButton, typeof(Button))]
[TemplatePart(PART_NoButton, typeof(Button))]
[TemplatePart(PART_OKButton, typeof(Button))]
[TemplatePart(PART_CancelButton, typeof(Button))]
public class StandardDrawerControl : DrawerControlBase
{
    public const string PART_YesButton = "PART_YesButton";
    public const string PART_NoButton = "PART_NoButton";
    public const string PART_OKButton = "PART_OKButton";
    public const string PART_CancelButton = "PART_CancelButton";

    public static readonly StyledProperty<DialogButton> ButtonsProperty =
        AvaloniaProperty.Register<StandardDrawerControl, DialogButton>(nameof(Buttons), DialogButton.OKCancel);

    public static readonly StyledProperty<DialogMode> ModeProperty =
        AvaloniaProperty.Register<StandardDrawerControl, DialogMode>(nameof(Mode));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<StandardDrawerControl, string?>(nameof(Title));

    public static readonly StyledProperty<ICommand?> TitleCommandProperty =
        AvaloniaProperty.Register<StandardDrawerControl, ICommand?>(nameof(TitleCommand));

    private Button? _yesButton;
    private Button? _noButton;
    private Button? _okButton;
    private Button? _cancelButton;

    public DialogButton Buttons
    {
        get => GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }

    public DialogMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ICommand? TitleCommand
    {
        get => GetValue(TitleCommandProperty);
        set => SetValue(TitleCommandProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Button.ClickEvent.RemoveHandler(OnButtonClick, _yesButton, _noButton, _okButton, _cancelButton);
        _yesButton = e.NameScope.Find<Button>(PART_YesButton);
        _noButton = e.NameScope.Find<Button>(PART_NoButton);
        _okButton = e.NameScope.Find<Button>(PART_OKButton);
        _cancelButton = e.NameScope.Find<Button>(PART_CancelButton);
        Button.ClickEvent.AddHandler(OnButtonClick, _yesButton, _noButton, _okButton, _cancelButton);
        SetButtonVisibility();
    }

    private void SetButtonVisibility()
    {
        if (_closeButton is not null)
            _closeButton.IsVisible =
                IsCloseButtonVisible ?? (DataContext is IDialogContext || Buttons != DialogButton.YesNo);
        SetVisible(_okButton, Buttons is DialogButton.OK or DialogButton.OKCancel);
        SetVisible(_cancelButton, Buttons is DialogButton.OKCancel or DialogButton.YesNoCancel);
        SetVisible(_yesButton, Buttons is DialogButton.YesNo or DialogButton.YesNoCancel);
        SetVisible(_noButton, Buttons is DialogButton.YesNo or DialogButton.YesNoCancel);
    }

    private static void SetVisible(Control? control, bool visible)
    {
        if (control is not null) control.IsVisible = visible;
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e) =>
        OnElementClosing(this,
            sender == _okButton ? DialogResult.OK :
            sender == _yesButton ? DialogResult.Yes :
            sender == _noButton ? DialogResult.No : DialogResult.Cancel);

    public override void Close()
    {
        if (DataContext is IDialogContext context) context.Close();
        else
            RaiseEvent(new ResultEventArgs(ClosedEvent, Buttons switch
            {
                DialogButton.OK => DialogResult.OK,
                DialogButton.YesNo => DialogResult.No,
                DialogButton.OKCancel or DialogButton.YesNoCancel => DialogResult.Cancel,
                _ => DialogResult.None
            }));
    }

    protected internal override void AnchorAndUpdatePositionInfo()
    {
    }
}