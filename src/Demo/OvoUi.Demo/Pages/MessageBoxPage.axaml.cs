using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using OvoUi.Controls;

namespace OvoUi.Demo.Pages;

public partial class MessageBoxPage : UserControl, INotifyPropertyChanged
{
    private const string LongMessage =
        "The selected deployment contains changes to authentication, storage, and background processing. " +
        "Review the release notes and verify that a rollback snapshot exists before continuing. " +
        "This longer message demonstrates wrapping and sizing in both overlay and standalone message boxes.";

    private string _messageTitle = "Confirm deployment";
    private string _messageContent = "Deploy version 2.4 to the production environment?";
    private MessageBoxIcon _selectedIcon = MessageBoxIcon.Question;
    private MessageBoxButton _selectedButtons = MessageBoxButton.YesNoCancel;
    private bool _useOverlay = true;
    private bool _showTitle = true;
    private string _lastResult = "None";

    public MessageBoxPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public IReadOnlyList<MessageBoxIcon> Icons { get; } = Enum.GetValues<MessageBoxIcon>();
    public IReadOnlyList<MessageBoxButton> ButtonSets { get; } = Enum.GetValues<MessageBoxButton>();

    public string MessageTitle
    {
        get => _messageTitle;
        set => SetField(ref _messageTitle, value);
    }

    public string MessageContent
    {
        get => _messageContent;
        set => SetField(ref _messageContent, value);
    }

    public MessageBoxIcon SelectedIcon
    {
        get => _selectedIcon;
        set => SetField(ref _selectedIcon, value);
    }

    public MessageBoxButton SelectedButtons
    {
        get => _selectedButtons;
        set => SetField(ref _selectedButtons, value);
    }

    public bool UseOverlay
    {
        get => _useOverlay;
        set => SetField(ref _useOverlay, value);
    }

    public bool ShowTitle
    {
        get => _showTitle;
        set => SetField(ref _showTitle, value);
    }

    public string LastResult
    {
        get => _lastResult;
        private set => SetField(ref _lastResult, value);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private async void ShowConfigured(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync(MessageContent, ShowTitle ? MessageTitle : string.Empty, SelectedIcon, SelectedButtons,
            UseOverlay);

    private async void ShowInformation(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync("The background index has finished updating.", "Index complete", MessageBoxIcon.Information,
            MessageBoxButton.OK, UseOverlay);

    private async void ShowSuccess(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync("All selected files were saved successfully.", "Changes saved", MessageBoxIcon.Success,
            MessageBoxButton.OK, UseOverlay);

    private async void ShowWarning(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync("The connection is unstable. Continue with the current operation?", "Connection warning",
            MessageBoxIcon.Warning, MessageBoxButton.OKCancel, UseOverlay);

    private async void ShowError(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync("The package could not be published. Review the build output and try again.", "Publish failed",
            MessageBoxIcon.Error, MessageBoxButton.OK, UseOverlay);

    private async void ShowQuestion(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync("Replace the existing file with the newer version?", "Confirm replacement",
            MessageBoxIcon.Question, MessageBoxButton.YesNoCancel, UseOverlay);

    private async void ShowLongMessage(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync(LongMessage, "Review deployment", MessageBoxIcon.Information, MessageBoxButton.OKCancel,
            UseOverlay);

    private async void ShowOk(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync(MessageContent, MessageTitle, SelectedIcon, MessageBoxButton.OK, UseOverlay);

    private async void ShowOkCancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync(MessageContent, MessageTitle, SelectedIcon, MessageBoxButton.OKCancel, UseOverlay);

    private async void ShowYesNo(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync(MessageContent, MessageTitle, SelectedIcon, MessageBoxButton.YesNo, UseOverlay);

    private async void ShowYesNoCancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync(MessageContent, MessageTitle, SelectedIcon, MessageBoxButton.YesNoCancel, UseOverlay);

    private async void ShowOverlayMessage(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync("The modal mask covers the full application window.", "Overlay message",
            MessageBoxIcon.Information, MessageBoxButton.OKCancel, true);

    private async void ShowOwnedWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync("Drag the title area to move this owned message window.", "Owned message window",
            MessageBoxIcon.Success, MessageBoxButton.OKCancel, false);

    private async void ShowUntitledWindow(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        await ShowAsync("This message uses the standalone window without title text.", string.Empty,
            MessageBoxIcon.None, MessageBoxButton.OK, false);

    private async Task ShowAsync(string message, string title, MessageBoxIcon icon, MessageBoxButton buttons,
        bool overlay)
    {
        if (overlay)
        {
            LastResult = (await OverlayMessageBox.ShowAsync(message, title, icon: icon, button: buttons)).ToString();
            return;
        }

        if (TopLevel.GetTopLevel(this) is not Window owner)
        {
            App.RootView.ToastManager.Show("Window message boxes are only available in the desktop host.",
                NotificationType.Warning);
            return;
        }

        LastResult = (await OvoUi.Controls.MessageBox.ShowAsync(owner, message, title, icon, buttons)).ToString();
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string ConfiguratorCode { get; } = """
                                              MessageBoxResult result = UseOverlay
                                                  ? await OverlayMessageBox.ShowAsync(MessageContent, MessageTitle,
                                                      icon: SelectedIcon, button: SelectedButtons)
                                                  : await MessageBox.ShowAsync(owner, MessageContent, MessageTitle,
                                                      SelectedIcon, SelectedButtons);
                                              """;

    public string PresetsCode { get; } = """
                                         await OverlayMessageBox.ShowAsync(
                                             "Replace the existing file with the newer version?",
                                             "Confirm replacement",
                                             icon: MessageBoxIcon.Question,
                                             button: MessageBoxButton.YesNoCancel);
                                         """;

    public string HostingCode { get; } = """
                                         // Overlay uses the full-window modal mask.
                                         await OverlayMessageBox.ShowAsync(message, title);

                                         // The owned window inherits its owner's typography and theme.
                                         await MessageBox.ShowAsync(owner, message, title,
                                             MessageBoxIcon.Information, MessageBoxButton.OKCancel);
                                         """;
}