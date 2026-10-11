using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Media;
using OvoUi.Controls;

namespace OvoUi.Demo.Pages;

public partial class ContentDialogPage : UserControl, INotifyPropertyChanged
{
    private string _dialogTitle = "Save changes";
    private string _dialogContent = "Choose how to continue before closing this document.";
    private string _primaryText = "Save";
    private string _secondaryText = "Don't save";
    private string _closeText = "Cancel";
    private ContentDialogButton _defaultButton = ContentDialogButton.Primary;
    private bool _fullSizeDesired;
    private string _lastResult = "None";
    private string _presetResult = "None";
    private string _validationResult = "Not started";

    public ContentDialogPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public IReadOnlyList<ContentDialogButton> DefaultButtons { get; } = Enum.GetValues<ContentDialogButton>();
    public string DialogTitle { get => _dialogTitle; set => SetField(ref _dialogTitle, value); }
    public string DialogContent { get => _dialogContent; set => SetField(ref _dialogContent, value); }
    public string PrimaryText { get => _primaryText; set => SetField(ref _primaryText, value); }
    public string SecondaryText { get => _secondaryText; set => SetField(ref _secondaryText, value); }
    public string CloseText { get => _closeText; set => SetField(ref _closeText, value); }
    public ContentDialogButton DefaultButton { get => _defaultButton; set => SetField(ref _defaultButton, value); }
    public bool FullSizeDesired { get => _fullSizeDesired; set => SetField(ref _fullSizeDesired, value); }
    public string LastResult { get => _lastResult; private set => SetField(ref _lastResult, value); }
    public string PresetResult { get => _presetResult; private set => SetField(ref _presetResult, value); }
    public string ValidationResult { get => _validationResult; private set => SetField(ref _validationResult, value); }
    public new event PropertyChangedEventHandler? PropertyChanged;

    private async void ShowConfigured(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var dialog = CreateDialog(DialogTitle, DialogContent, PrimaryText, SecondaryText, CloseText);
        dialog.DefaultButton = DefaultButton;
        dialog.FullSizeDesired = FullSizeDesired;
        LastResult = (await dialog.ShowAsync()).ToString();
    }

    private async void ShowDeleteConfirm(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var content = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new TextBlock { Text = "Delete 'Quarterly report.docx'?", TextWrapping = TextWrapping.Wrap },
                new TextBlock { Text = "The file can be restored from the recycle bin for 30 days.", Opacity = 0.65, TextWrapping = TextWrapping.Wrap }
            }
        };
        var dialog = new ContentDialog { Title = "Delete file", Content = content, PrimaryButtonText = "Delete", CloseButtonText = "Cancel", DefaultButton = ContentDialogButton.Close };
        PresetResult = await dialog.ShowAsync() == ContentDialogResult.Primary ? "File deleted" : "Deletion cancelled";
    }

    private async void ShowSaveChanges(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var result = await CreateDialog("Save changes", "This document contains unsaved changes.", "Save", "Don't save", "Cancel").ShowAsync();
        PresetResult = result switch
        {
            ContentDialogResult.Primary => "Saved and closed",
            ContentDialogResult.Secondary => "Closed without saving",
            _ => "Returned to the document"
        };
    }

    private async void ShowInputDialog(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var input = new TextBox { MinWidth = 320, PlaceholderText = "Project name", Text = "OvoUi" };
        var dialog = new ContentDialog
        {
            Title = "Rename project",
            Content = new StackPanel { Spacing = 8, Children = { new TextBlock { Text = "Enter a new project name." }, input } },
            PrimaryButtonText = "Rename",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };
        dialog.Opened += (_, _) => input.Focus();
        var result = await dialog.ShowAsync();
        PresetResult = result == ContentDialogResult.Primary ? $"Renamed to {input.Text}" : "Rename cancelled";
    }

    private async void ShowRichContent(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var environment = new ComboBox { ItemsSource = new[] { "Production", "Staging", "Development" }, SelectedIndex = 1 };
        var backup = new CheckBox { Content = "Create a backup before deployment", IsChecked = true };
        var notes = new TextBox { AcceptsReturn = true, MinHeight = 72, PlaceholderText = "Release notes", TextWrapping = TextWrapping.Wrap };
        var dialog = new ContentDialog
        {
            Title = "Deploy release",
            Content = new StackPanel
            {
                Spacing = 10,
                Children = { new TextBlock { Text = "Environment" }, environment, backup, new TextBlock { Text = "Notes" }, notes }
            },
            PrimaryButtonText = "Deploy",
            SecondaryButtonText = "Save draft",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };
        var result = await dialog.ShowAsync();
        PresetResult = $"{result}: {environment.SelectedItem}";
    }

    private async void ShowCloseOnly(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var dialog = new ContentDialog { Title = "Session notice", Content = "Your session will expire in five minutes.", CloseButtonText = "Got it", DefaultButton = ContentDialogButton.Close };
        await dialog.ShowAsync();
        PresetResult = "Notice acknowledged";
    }

    private async void ShowValidatedDialog(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var input = new TextBox { MinWidth = 320, PlaceholderText = "Type CONFIRM" };
        var status = new TextBlock { Opacity = 0.7 };
        var dialog = new ContentDialog
        {
            Title = "Confirm operation",
            Content = new StackPanel { Spacing = 8, Children = { input, status } },
            PrimaryButtonText = "Continue",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };
        dialog.PrimaryButtonClick += async (_, args) =>
        {
            var deferral = args.GetDeferral();
            status.Text = "Validating...";
            await Task.Delay(500);
            args.Cancel = !string.Equals(input.Text, "CONFIRM", StringComparison.OrdinalIgnoreCase);
            status.Text = args.Cancel ? "Enter CONFIRM before continuing." : "Validated";
            deferral.Complete();
        };
        ValidationResult = (await dialog.ShowAsync()).ToString();
    }

    private static ContentDialog CreateDialog(string title, string content, string? primary, string? secondary, string? close) => new()
    {
        Title = title,
        Content = new TextBlock { Text = content, MaxWidth = 460, TextWrapping = TextWrapping.Wrap },
        PrimaryButtonText = NullIfEmpty(primary),
        SecondaryButtonText = NullIfEmpty(secondary),
        CloseButtonText = NullIfEmpty(close),
        DefaultButton = ContentDialogButton.Primary
    };

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string ConfiguratorCode { get; } = """
        var dialog = new ContentDialog
        {
            Title = DialogTitle,
            Content = DialogContent,
            PrimaryButtonText = PrimaryText,
            SecondaryButtonText = SecondaryText,
            CloseButtonText = CloseText,
            DefaultButton = DefaultButton,
            FullSizeDesired = FullSizeDesired
        };
        ContentDialogResult result = await dialog.ShowAsync();
        """;

    public string ContentCode { get; } = """
        var input = new TextBox { PlaceholderText = "Project name" };
        var dialog = new ContentDialog
        {
            Title = "Rename project",
            Content = input,
            PrimaryButtonText = "Rename",
            CloseButtonText = "Cancel"
        };
        ContentDialogResult result = await dialog.ShowAsync();
        """;

    public string DeferralCode { get; } = """
        dialog.PrimaryButtonClick += async (_, args) =>
        {
            Deferral deferral = args.GetDeferral();
            await ValidateAsync();
            args.Cancel = input.Text != "CONFIRM";
            deferral.Complete();
        };
        """;
}
