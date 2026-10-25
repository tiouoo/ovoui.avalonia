using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using OvoUi.Common.Classes;
using OvoUi.Controls;
using OvoNotification = OvoUi.Controls.Notification;

namespace OvoUi.Demo.Pages;

public partial class NotificationPage : UserControl, INotifyPropertyChanged
{
    private string _notificationTitle = "Update available";
    private string _notificationContent = "Version 2.4 is ready to install.";
    private NotificationType _selectedType = NotificationType.Information;
    private NotificationPosition _selectedPosition = NotificationPosition.TopRight;
    private decimal _expirationSeconds = 6;
    private bool _isIconVisible = true;
    private bool _isCloseButtonVisible = true;
    private bool _isCollapseButtonVisible;
    private bool _isTouchClose;
    private bool _isColorful = true;
    private bool _includeActions;
    private bool _isButtonsInline = true;
    private string _lastCloseReason = "None";
    private string _lastInteraction = "None";
    private OvoNotification? _trackedNotification;

    public NotificationPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    private OvoNotificationManager Manager => App.RootView.NotificationManager;
    public IReadOnlyList<NotificationType> Types { get; } = Enum.GetValues<NotificationType>();
    public IReadOnlyList<NotificationPosition> Positions { get; } = Enum.GetValues<NotificationPosition>();

    public string NotificationTitle
    {
        get => _notificationTitle;
        set => SetField(ref _notificationTitle, value);
    }

    public string NotificationContent
    {
        get => _notificationContent;
        set => SetField(ref _notificationContent, value);
    }

    public NotificationType SelectedType
    {
        get => _selectedType;
        set => SetField(ref _selectedType, value);
    }

    public NotificationPosition SelectedPosition
    {
        get => _selectedPosition;
        set => SetField(ref _selectedPosition, value);
    }

    public decimal ExpirationSeconds
    {
        get => _expirationSeconds;
        set => SetField(ref _expirationSeconds, value);
    }

    public bool IsIconVisible
    {
        get => _isIconVisible;
        set => SetField(ref _isIconVisible, value);
    }

    public bool IsCloseButtonVisible
    {
        get => _isCloseButtonVisible;
        set => SetField(ref _isCloseButtonVisible, value);
    }

    public bool IsCollapseButtonVisible
    {
        get => _isCollapseButtonVisible;
        set => SetField(ref _isCollapseButtonVisible, value);
    }

    public bool IsTouchClose
    {
        get => _isTouchClose;
        set => SetField(ref _isTouchClose, value);
    }

    public bool IsColorful
    {
        get => _isColorful;
        set => SetField(ref _isColorful, value);
    }

    public bool IncludeActions
    {
        get => _includeActions;
        set => SetField(ref _includeActions, value);
    }

    public bool IsButtonsInline
    {
        get => _isButtonsInline;
        set => SetField(ref _isButtonsInline, value);
    }

    public string LastCloseReason
    {
        get => _lastCloseReason;
        private set => SetField(ref _lastCloseReason, value);
    }

    public string LastInteraction
    {
        get => _lastInteraction;
        private set => SetField(ref _lastInteraction, value);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void ShowConfigured(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Manager.Position = SelectedPosition;
        Manager.Show(CreateOptions());
    }

    private NotificationOptions CreateOptions() => new()
    {
        Title = string.IsNullOrWhiteSpace(NotificationTitle) ? null : NotificationTitle,
        Content = NotificationContent,
        Type = SelectedType,
        Expiration = ExpirationSeconds <= 0 ? TimeSpan.Zero : TimeSpan.FromSeconds((double)ExpirationSeconds),
        IsIconVisible = IsIconVisible,
        IsCloseButtonVisible = IsCloseButtonVisible,
        IsCollapseButtonVisible = IsCollapseButtonVisible,
        IsTouchClose = IsTouchClose,
        IsColorful = IsColorful,
        IsButtonsInline = IsButtonsInline,
        OperateButtons = IncludeActions ? CreateActions() : null,
        OnClick = () => LastInteraction = "Notification clicked",
        OnClose = SetCloseReason
    };

    private ObservableCollection<OperateButtonEntry> CreateActions() =>
    [
        new("Open", _ => LastInteraction = "Open selected"),
        new("Dismiss", _ => LastInteraction = "Dismiss selected", closeOnClick: true)
    ];

    private void ShowInformation(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        ShowPreset("Build started", "The release pipeline is now running.", NotificationType.Information);

    private void ShowSuccess(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        ShowPreset("Deployment complete", "Version 2.4 is live in production.", NotificationType.Success);

    private void ShowWarning(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        ShowPreset("Storage warning", "Only 12% of the available capacity remains.", NotificationType.Warning);

    private void ShowError(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        ShowPreset("Deployment failed", "Review the service logs before retrying.", NotificationType.Error);

    private void ShowPreset(string title, string content, NotificationType type) =>
        Manager.Show(title, content, new NotificationOptions { Type = type, OnClose = SetCloseReason });

    private void ShowTitleOnly(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Manager.Show(
        new NotificationOptions
        {
            Title = "All downloads complete", Type = NotificationType.Success, OnClose = SetCloseReason
        });

    private void ShowLongContent(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Manager.Show(
        new NotificationOptions
        {
            Title = "Review required before publishing the release",
            Content =
                "Authentication settings changed.\nStorage migrations are pending.\nA rollback snapshot is available for 24 hours.",
            Type = NotificationType.Warning,
            Expiration = TimeSpan.FromSeconds(10),
            OnClose = SetCloseReason
        });

    private void ShowInlineActions(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowActions(true);
    private void ShowStackedActions(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowActions(false);

    private void ShowActions(bool inline) => Manager.Show(new NotificationOptions
    {
        Title = "Restart required",
        Content = "Restart the application to finish installing the update.",
        Type = NotificationType.Information,
        Expiration = TimeSpan.FromSeconds(12),
        IsButtonsInline = inline,
        OperateButtons = new ObservableCollection<OperateButtonEntry>
        {
            new("Restart", _ => LastInteraction = "Restart selected", closeOnClick: true),
            new("Later", _ => LastInteraction = "Later selected", closeOnClick: true),
            new("Details", _ => LastInteraction = "Details selected")
        },
        OnClose = SetCloseReason
    });

    private void ShowLightStyle(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var options = StyleOptions("Light notification", NotificationType.Information);
        options.Classes.Clear();
        options.Classes.Add("Light");
        Manager.Show(options);
    }

    private void ShowDefaultStyle(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var options = StyleOptions("Default notification", NotificationType.Warning);
        options.Classes.Clear();
        Manager.Show(options);
    }

    private void ShowColorfulStyle(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        Manager.Show(StyleOptions("Colorful notification", NotificationType.Success, true));

    private NotificationOptions StyleOptions(string content, NotificationType type, bool colorful = false) => new()
    {
        Title = "Appearance",
        Content = content,
        Type = type,
        IsColorful = colorful,
        OnClose = SetCloseReason
    };

    private void ShowClickable(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Manager.Show(
        new NotificationOptions
        {
            Title = "Build output ready",
            Content = "Click anywhere on this notification to open the build summary.",
            Type = NotificationType.Information,
            Expiration = TimeSpan.FromSeconds(10),
            IsTouchClose = true,
            OnClick = () => LastInteraction = "Build summary requested",
            OnClose = SetCloseReason
        });

    private void MoveTopLeft(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        MoveAndShow(NotificationPosition.TopLeft);

    private void MoveTopCenter(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        MoveAndShow(NotificationPosition.TopCenter);

    private void MoveTopRight(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        MoveAndShow(NotificationPosition.TopRight);

    private void MoveBottomLeft(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        MoveAndShow(NotificationPosition.BottomLeft);

    private void MoveBottomCenter(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        MoveAndShow(NotificationPosition.BottomCenter);

    private void MoveBottomRight(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        MoveAndShow(NotificationPosition.BottomRight);

    private void MoveAndShow(NotificationPosition position)
    {
        Manager.Position = position;
        SelectedPosition = position;
        ShowPreset("Position changed", $"Notifications now appear at {position}.", NotificationType.Information);
    }

    private void ShowTimeout(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Manager.Show(
        new NotificationOptions
        {
            Title = "Timeout example",
            Content = "This notification closes automatically after two seconds.",
            Expiration = TimeSpan.FromSeconds(2),
            OnClose = SetCloseReason
        });

    private void ShowPersistent(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Manager.Show(
        new NotificationOptions
        {
            Title = "Persistent notification",
            Content = "Use the close button or Close all to dismiss this item.",
            Expiration = TimeSpan.Zero,
            OnClose = SetCloseReason
        });

    private void ShowTracked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _trackedNotification = new OvoNotification(
            "Tracked notification",
            "Use Close tracked to close this specific notification.",
            NotificationType.Information,
            TimeSpan.Zero,
            showClose: true,
            onClose: SetCloseReason)
        {
            ShowIcon = true
        };
        Manager.Show(_trackedNotification);
    }

    private void CloseTracked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_trackedNotification is null)
        {
            LastInteraction = "No tracked notification";
            return;
        }

        Manager.Close(_trackedNotification);
        _trackedNotification = null;
    }

    private void ShowDisplaced(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        for (var i = 1; i <= Manager.MaxItems + 2; i++)
        {
            var index = i;
            Manager.Show(new NotificationOptions
            {
                Title = $"Queued notification {index}",
                Content = "Older items are displaced when the visible limit is exceeded.",
                Expiration = TimeSpan.Zero,
                OnClose = reason => LastCloseReason = $"#{index}: {reason}"
            });
        }
    }

    private void CloseAll(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Manager.CloseAll();
        _trackedNotification = null;
    }

    private void SetCloseReason(MessageCloseReason reason) => LastCloseReason = reason.ToString();

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string ConfiguratorCode { get; } = """
                                              manager.Position = SelectedPosition;
                                              manager.Show(new NotificationOptions
                                              {
                                                  Title = NotificationTitle,
                                                  Content = NotificationContent,
                                                  Type = SelectedType,
                                                  Expiration = TimeSpan.FromSeconds((double)ExpirationSeconds),
                                                  IsIconVisible = IsIconVisible,
                                                  IsCloseButtonVisible = IsCloseButtonVisible,
                                                  IsTouchClose = IsTouchClose,
                                                  IsColorful = IsColorful,
                                                  IsButtonsInline = IsButtonsInline,
                                                  OperateButtons = actions,
                                                  OnClose = reason => LastCloseReason = reason.ToString()
                                              });
                                              """;

    public string PresetsCode { get; } = """
                                         manager.Show(new NotificationOptions
                                         {
                                             Title = "Restart required",
                                             Content = "Restart to finish installing the update.",
                                             IsButtonsInline = true,
                                             OperateButtons = new ObservableCollection<OperateButtonEntry>
                                             {
                                                 new("Restart", _ => Restart(), closeOnClick: true),
                                                 new("Later", _ => { }, closeOnClick: true)
                                             }
                                         });
                                         """;

    public string LifetimeCode { get; } = """
                                          var notification = new Notification(
                                              "Tracked notification", message,
                                              expiration: TimeSpan.Zero,
                                              onClose: reason => LastCloseReason = reason.ToString());

                                          manager.Show(notification);
                                          manager.Close(notification);
                                          manager.CloseAll();
                                          """;
}