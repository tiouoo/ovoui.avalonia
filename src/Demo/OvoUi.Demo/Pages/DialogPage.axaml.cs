using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using OvoUi.Common.Enums;
using OvoUi.Controls;

namespace OvoUi.Demo.Pages;

public partial class DialogPage : UserControl, INotifyPropertyChanged
{
    private const string LocalHostId = "DialogPage.Local";
    private string _dialogTitle = "Review changes";
    private string _dialogContent = "Review the selected files before continuing.";
    private DialogMode _mode = DialogMode.Info;
    private DialogButton _buttons = DialogButton.OKCancel;
    private HorizontalPosition _horizontalAnchor = HorizontalPosition.Center;
    private VerticalPosition _verticalAnchor = VerticalPosition.Center;
    private bool _useWindow;
    private bool _isCustom;
    private bool _isModal = true;
    private bool _canLightDismiss;
    private bool _canDragMove = true;
    private bool _canResize;
    private bool _fullScreen;
    private bool _useLocalHost;
    private bool? _isCloseButtonVisible = true;
    private string _lastResult = "None";

    public DialogPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public IReadOnlyList<DialogMode> Modes { get; } = Enum.GetValues<DialogMode>();
    public IReadOnlyList<DialogButton> ButtonSets { get; } = Enum.GetValues<DialogButton>();
    public IReadOnlyList<HorizontalPosition> HorizontalAnchors { get; } = Enum.GetValues<HorizontalPosition>();
    public IReadOnlyList<VerticalPosition> VerticalAnchors { get; } = Enum.GetValues<VerticalPosition>();

    public string DialogTitle
    {
        get => _dialogTitle;
        set => SetField(ref _dialogTitle, value);
    }

    public string DialogContent
    {
        get => _dialogContent;
        set => SetField(ref _dialogContent, value);
    }

    public DialogMode Mode
    {
        get => _mode;
        set => SetField(ref _mode, value);
    }

    public DialogButton Buttons
    {
        get => _buttons;
        set => SetField(ref _buttons, value);
    }

    public HorizontalPosition HorizontalAnchor
    {
        get => _horizontalAnchor;
        set => SetField(ref _horizontalAnchor, value);
    }

    public VerticalPosition VerticalAnchor
    {
        get => _verticalAnchor;
        set => SetField(ref _verticalAnchor, value);
    }

    public bool UseWindow
    {
        get => _useWindow;
        set => SetField(ref _useWindow, value);
    }

    public bool IsCustom
    {
        get => _isCustom;
        set => SetField(ref _isCustom, value);
    }

    public bool IsModal
    {
        get => _isModal;
        set => SetField(ref _isModal, value);
    }

    public bool CanLightDismiss
    {
        get => _canLightDismiss;
        set => SetField(ref _canLightDismiss, value);
    }

    public bool CanDragMove
    {
        get => _canDragMove;
        set => SetField(ref _canDragMove, value);
    }

    public bool CanResize
    {
        get => _canResize;
        set => SetField(ref _canResize, value);
    }

    public bool FullScreen
    {
        get => _fullScreen;
        set => SetField(ref _fullScreen, value);
    }

    public bool UseLocalHost
    {
        get => _useLocalHost;
        set => SetField(ref _useLocalHost, value);
    }

    public bool? IsCloseButtonVisible
    {
        get => _isCloseButtonVisible;
        set => SetField(ref _isCloseButtonVisible, value);
    }

    public string LastResult
    {
        get => _lastResult;
        private set => SetField(ref _lastResult, value);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private async void ShowConfigured(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (UseWindow)
            await ShowConfiguredWindow();
        else
            await ShowConfiguredOverlay();
    }

    private async Task ShowConfiguredOverlay()
    {
        var options = new OverlayDialogOptions
        {
            Title = DialogTitle,
            Mode = Mode,
            Buttons = Buttons,
            HorizontalAnchor = HorizontalAnchor,
            VerticalAnchor = VerticalAnchor,
            CanLightDismiss = CanLightDismiss,
            CanDragMove = CanDragMove,
            CanResize = CanResize,
            FullScreen = FullScreen,
            IsCloseButtonVisible = IsCloseButtonVisible
        };
        var hostId = UseLocalHost ? LocalHostId : null;
        if (IsCustom)
        {
            var content = CreateCustomContent(DialogContent);
            if (IsModal)
                LastResult = await OverlayDialog.ShowCustomAsync<string>(content, null, hostId, options) ?? "None";
            else
            {
                OverlayDialog.ShowCustom(content, null, hostId, options);
                LastResult = "Opened non-modal";
            }
        }
        else if (IsModal)
        {
            LastResult =
                (await OverlayDialog.ShowStandardAsync(CreateTextContent(DialogContent), null, hostId, options))
                .ToString();
        }
        else
        {
            OverlayDialog.ShowStandard(CreateTextContent(DialogContent), null, hostId, options);
            LastResult = "Opened non-modal";
        }
    }

    private async Task ShowConfiguredWindow()
    {
        if (TopLevel.GetTopLevel(this) is not Window owner)
        {
            App.RootView.ToastManager.Show("Window dialogs are only available in the desktop host.",
                NotificationType.Warning);
            return;
        }

        var options = new DialogOptions
        {
            Title = DialogTitle,
            Mode = Mode,
            Buttons = Buttons,
            CanDragMove = CanDragMove,
            CanResize = CanResize,
            IsCloseButtonVisible = IsCloseButtonVisible,
            DialogWindowWidth = 520,
            DialogWindowHeight = 360
        };
        if (IsCustom)
        {
            var content = CreateCustomContent(DialogContent);
            if (IsModal)
                LastResult = await OvoUi.Controls.Dialog.ShowCustomAsync<string>(content, null, owner, options) ??
                             "None";
            else
            {
                OvoUi.Controls.Dialog.ShowCustom(content, null, owner, options);
                LastResult = "Opened non-modal window";
            }
        }
        else
        {
            LastResult =
                (await OvoUi.Controls.Dialog.ShowStandardAsync(CreateTextContent(DialogContent), null, owner, options))
                .ToString();
        }
    }

    private async void ShowModalMask(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LastResult = (await OverlayDialog.ShowStandardAsync(
            CreateTextContent("Drag the dimmed mask to move the application window."), null,
            options: new OverlayDialogOptions
            {
                Title = "Modal dialog", Mode = DialogMode.Info, Buttons = DialogButton.OKCancel,
                CanLightDismiss = false, CanDragMove = true
            })).ToString();
    }

    private async void ShowLightDismiss(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        LastResult = (await OverlayDialog.ShowStandardAsync(CreateTextContent("Click outside the dialog to close it."),
            null, options: new OverlayDialogOptions
            {
                Title = "Light dismiss", Buttons = DialogButton.OK, CanLightDismiss = true
            })).ToString();

    private void ShowNonModal(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        OverlayDialog.ShowStandard(CreateTextContent("The page remains interactive while this dialog is open."), null,
            options: new OverlayDialogOptions
            {
                Title = "Non-modal dialog", Mode = DialogMode.Success, Buttons = DialogButton.OK, CanDragMove = true
            });

    private async void ShowFullScreen(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        LastResult = (await OverlayDialog.ShowStandardAsync(
            CreateTextContent("Full-screen content uses the available overlay host."), null,
            options: new OverlayDialogOptions
            {
                Title = "Full-screen dialog", Buttons = DialogButton.OK, FullScreen = true
            })).ToString();

    private async void ShowWindowDialog(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UseWindow = true;
        IsCustom = false;
        await ShowConfiguredWindow();
    }

    private async void ShowWindowForm(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UseWindow = true;
        IsCustom = true;
        await ShowConfiguredWindow();
    }

    private static Control CreateTextContent(string text) => new TextBlock
        { Text = text, MaxWidth = 500, TextWrapping = TextWrapping.Wrap };

    private static Control CreateCustomContent(string text)
    {
        var name = new TextBox { PlaceholderText = "Display name", Text = "Ada" };
        var role = new ComboBox { ItemsSource = new[] { "Viewer", "Editor", "Administrator" }, SelectedIndex = 1 };
        var close = new Button
        {
            Content = "Close", Classes = { "accent" }, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
        DialogControlBase.SetCanClose(close, true);
        return new StackPanel
        {
            Margin = new Thickness(28),
            MinWidth = 360,
            Spacing = 12,
            Children =
            {
                new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap }, name, role,
                new CheckBox { Content = "Send a confirmation email", IsChecked = true }, close
            }
        };
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string ConfiguratorCode { get; } = """
                                              var options = new OverlayDialogOptions
                                              {
                                                  Title = DialogTitle,
                                                  Mode = Mode,
                                                  Buttons = Buttons,
                                                  CanLightDismiss = CanLightDismiss,
                                                  CanDragMove = CanDragMove,
                                                  CanResize = CanResize,
                                                  FullScreen = FullScreen
                                              };
                                              DialogResult result = await OverlayDialog.ShowStandardAsync(content, null, hostId, options);
                                              """;

    public string PresetsCode { get; } = """
                                         // A modal mask can move the top-level window when light dismiss is disabled.
                                         await OverlayDialog.ShowStandardAsync(content, null, options: new OverlayDialogOptions
                                         {
                                             CanLightDismiss = false,
                                             CanDragMove = true
                                         });
                                         """;
}