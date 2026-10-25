using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using OvoUi.Common;
using OvoUi.Common.Enums;
using OvoUi.Controls;
using OvoUi.Controls.Options;

namespace OvoUi.Demo.Pages;

public partial class DrawerPage : UserControl, INotifyPropertyChanged
{
    private const string LocalHostId = "DrawerPage.Local";
    private string _drawerTitle = "Edit profile";
    private string _drawerContent = "Update the profile fields and choose an action.";
    private Position _position = Position.Right;
    private DialogButton _buttons = DialogButton.OKCancel;
    private bool _isModal = true;
    private bool _isCustom;
    private bool _canLightDismiss = true;
    private bool _canResize;
    private bool _useLocalHost;
    private bool? _isCloseButtonVisible = true;
    private string _lastResult = "None";

    public DrawerPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public IReadOnlyList<Position> Positions { get; } = Enum.GetValues<Position>();
    public IReadOnlyList<DialogButton> ButtonSets { get; } = Enum.GetValues<DialogButton>();

    public string DrawerTitle
    {
        get => _drawerTitle;
        set => SetField(ref _drawerTitle, value);
    }

    public string DrawerContent
    {
        get => _drawerContent;
        set => SetField(ref _drawerContent, value);
    }

    public Position Position
    {
        get => _position;
        set => SetField(ref _position, value);
    }

    public DialogButton Buttons
    {
        get => _buttons;
        set => SetField(ref _buttons, value);
    }

    public bool IsModal
    {
        get => _isModal;
        set => SetField(ref _isModal, value);
    }

    public bool IsCustom
    {
        get => _isCustom;
        set => SetField(ref _isCustom, value);
    }

    public bool CanLightDismiss
    {
        get => _canLightDismiss;
        set => SetField(ref _canLightDismiss, value);
    }

    public bool CanResize
    {
        get => _canResize;
        set => SetField(ref _canResize, value);
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
        var options = CreateOptions(Position);
        var hostId = UseLocalHost ? LocalHostId : null;
        var content = IsCustom ? CreateSettingsContent() : CreateDetailsContent(DrawerContent);
        if (IsCustom)
        {
            if (IsModal)
                LastResult = await OverlayDrawer.ShowCustomAsync<string>(content, null, hostId, options) ?? "None";
            else
            {
                OverlayDrawer.ShowCustom(content, null, hostId, options);
                LastResult = "Opened non-modal";
            }
        }
        else if (IsModal)
        {
            LastResult = (await OverlayDrawer.ShowStandardAsync(content, null, hostId, options)).ToString();
        }
        else
        {
            OverlayDrawer.ShowStandard(content, null, hostId, options);
            LastResult = "Opened non-modal";
        }
    }

    private void ShowLeft(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowPreset(Position.Left);
    private void ShowTop(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowPreset(Position.Top);
    private void ShowRight(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowPreset(Position.Right);
    private void ShowBottom(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowPreset(Position.Bottom);

    private void ShowPreset(Position position)
    {
        OverlayDrawer.ShowStandard(
            CreateDetailsContent($"Content enters from the {position.ToString().ToLowerInvariant()} edge."), null,
            options: new DrawerOptions
            {
                Position = position,
                Title = $"{position} drawer",
                Buttons = DialogButton.OK,
                CanLightDismiss = true,
                MinWidth = position is Position.Left or Position.Right ? 340 : null,
                MinHeight = position is Position.Top or Position.Bottom ? 220 : null
            });
    }

    private void ShowSettings(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        OverlayDrawer.ShowCustom(CreateSettingsContent(), null, options: new DrawerOptions
        {
            Position = Position.Right, CanLightDismiss = true, MinWidth = 380, IsCloseButtonVisible = true
        });

    private async void ShowDetails(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        LastResult = (await OverlayDrawer.ShowStandardAsync(
            CreateDetailsContent("Review deployment details before publishing."), null, options: new DrawerOptions
            {
                Position = Position.Right, Title = "Publish release", Buttons = DialogButton.YesNoCancel,
                CanLightDismiss = false, MinWidth = 420
            })).ToString();

    private DrawerOptions CreateOptions(Position position) => new()
    {
        Position = position,
        Title = DrawerTitle,
        Buttons = Buttons,
        CanLightDismiss = CanLightDismiss,
        CanResize = CanResize,
        IsCloseButtonVisible = IsCloseButtonVisible,
        MinWidth = position is Position.Left or Position.Right ? 360 : null,
        MinHeight = position is Position.Top or Position.Bottom ? 240 : null
    };

    private static Control CreateDetailsContent(string text) => new StackPanel
    {
        Spacing = 10,
        Children =
        {
            new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap },
            new TextBlock { Text = "Changed files: 12", Opacity = 0.7 },
            new TextBlock { Text = "Target: Production", Opacity = 0.7 },
            new CheckBox { Content = "Create rollback snapshot", IsChecked = true }
        }
    };

    private static Control CreateSettingsContent()
    {
        var close = new Button
        {
            Classes = { "accent" }, Content = "Apply", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
        DialogControlBase.SetCanClose(close, true);
        return new StackPanel
        {
            Margin = new Thickness(24),
            MinWidth = 320,
            Spacing = 10,
            Children =
            {
                new TextBlock { Text = "Profile settings", FontSize = 18, FontWeight = FontWeight.SemiBold },
                new TextBox { PlaceholderText = "Display name", Text = "Ada Lovelace" },
                new ComboBox { ItemsSource = new[] { "Engineering", "Design", "Operations" }, SelectedIndex = 0 },
                new ToggleSwitch { Content = "Email notifications", IsChecked = true },
                new CheckBox { Content = "Make profile public" },
                close
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
                                              var options = new DrawerOptions
                                              {
                                                  Position = Position,
                                                  Title = DrawerTitle,
                                                  Buttons = Buttons,
                                                  CanLightDismiss = CanLightDismiss,
                                                  CanResize = CanResize,
                                                  IsCloseButtonVisible = IsCloseButtonVisible
                                              };
                                              DialogResult result = await OverlayDrawer.ShowStandardAsync(content, null, hostId, options);
                                              """;

    public string PresetsCode { get; } = """
                                         OverlayDrawer.ShowCustom(settingsForm, null, options: new DrawerOptions
                                         {
                                             Position = Position.Right,
                                             CanLightDismiss = true,
                                             MinWidth = 380
                                         });
                                         """;
}