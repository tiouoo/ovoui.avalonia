using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using OvoUi.Common.Classes;
using OvoUi.Controls;

namespace OvoUi.Demo.Pages;

public partial class ToastPage : UserControl, INotifyPropertyChanged
{
    private string _toastContent = "Changes saved successfully.";
    private NotificationType _selectedType = NotificationType.Success;
    private decimal _expirationSeconds = 4;
    private bool _isIconVisible = true;
    private bool _isCloseButtonVisible = true;
    private bool _isCollapseButtonVisible;
    private bool _isTouchClose;
    private bool _isColorful = true;
    private bool _includeActions;
    private bool _isButtonsInline = true;
    private string _lastCloseReason = "None";
    private string _lastInteraction = "None";
    private Toast? _trackedToast;

    public ToastPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    private OvoToastManager Manager => App.RootView.ToastManager;
    public IReadOnlyList<NotificationType> Types { get; } = Enum.GetValues<NotificationType>();

    public string ToastContent
    {
        get => _toastContent;
        set => SetField(ref _toastContent, value);
    }

    public NotificationType SelectedType
    {
        get => _selectedType;
        set => SetField(ref _selectedType, value);
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

    private void ShowConfigured(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Manager.Show(
        new NotificationOptions
        {
            Content = ToastContent,
            Type = SelectedType,
            Expiration = ExpirationSeconds <= 0 ? TimeSpan.Zero : TimeSpan.FromSeconds((double)ExpirationSeconds),
            IsIconVisible = IsIconVisible,
            IsCloseButtonVisible = IsCloseButtonVisible,
            IsCollapseButtonVisible = IsCollapseButtonVisible,
            IsTouchClose = IsTouchClose,
            IsColorful = IsColorful,
            IsButtonsInline = IsButtonsInline,
            OperateButtons = IncludeActions ? CreateActions() : null,
            OnClick = () => LastInteraction = "Toast clicked",
            OnClose = SetCloseReason
        });

    private ObservableCollection<OperateButtonEntry> CreateActions() =>
    [
        new("Open", _ => LastInteraction = "Open selected"),
        new("Dismiss", _ => LastInteraction = "Dismiss selected", closeOnClick: true)
    ];

    private void ShowInformation(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        ShowPreset("A background task has started.", NotificationType.Information);

    private void ShowSuccess(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        ShowPreset("Changes saved successfully.", NotificationType.Success);

    private void ShowWarning(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        ShowPreset("The connection is unstable.", NotificationType.Warning);

    private void ShowError(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        ShowPreset("The request could not be completed.", NotificationType.Error);

    private void ShowPreset(string content, NotificationType type) => Manager.Show(new NotificationOptions
    {
        Content = content,
        Type = type,
        OnClose = SetCloseReason
    });

    private void ShowLongContent(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Manager.Show(
        new NotificationOptions
        {
            Content =
                "The export completed with warnings. Three records used fallback values and two attachments were skipped because they exceeded the configured size limit.",
            Type = NotificationType.Warning,
            Expiration = TimeSpan.FromSeconds(8),
            OnClose = SetCloseReason
        });

    private void ShowMultiline(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Manager.Show(
        new NotificationOptions
        {
            Content = "Upload complete\n12 files processed\n2 warnings require review",
            Type = NotificationType.Success,
            Expiration = TimeSpan.FromSeconds(6),
            OnClose = SetCloseReason
        });

    private void ShowOneSecond(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowDuration(1);
    private void ShowThreeSeconds(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowDuration(3);
    private void ShowFiveSeconds(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowDuration(5);
    private void ShowPersistent(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowDuration(0);

    private void ShowDuration(int seconds) => Manager.Show(new NotificationOptions
    {
        Content = seconds == 0
            ? "Persistent toast: close this item manually."
            : $"This toast remains visible for {seconds} second(s).",
        Type = NotificationType.Information,
        Expiration = TimeSpan.FromSeconds(seconds),
        OnClose = SetCloseReason
    });

    private void ShowWithoutIcon(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Manager.Show(
        new NotificationOptions
        {
            Content = "Toast without a status icon.",
            IsIconVisible = false,
            OnClose = SetCloseReason
        });

    private void ShowWithoutClose(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Manager.Show(
        new NotificationOptions
        {
            Content = "Toast without a close button.",
            IsCloseButtonVisible = false,
            Expiration = TimeSpan.FromSeconds(3),
            OnClose = SetCloseReason
        });

    private void ShowInlineActions(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowActions(true);
    private void ShowStackedActions(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowActions(false);

    private void ShowActions(bool inline) => Manager.Show(new NotificationOptions
    {
        Content = "The download is ready.",
        Type = NotificationType.Information,
        Expiration = TimeSpan.FromSeconds(10),
        IsButtonsInline = inline,
        OperateButtons = new ObservableCollection<OperateButtonEntry>
        {
            new("Open", _ => LastInteraction = "Open selected", closeOnClick: true),
            new("Folder", _ => LastInteraction = "Folder selected"),
            new("Dismiss", _ => LastInteraction = "Dismiss selected", closeOnClick: true)
        },
        OnClose = SetCloseReason
    });

    private void ShowLightStyle(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var options = StyleOptions("Light toast", NotificationType.Information);
        options.Classes.Clear();
        options.Classes.Add("Light");
        Manager.Show(options);
    }

    private void ShowDefaultStyle(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var options = StyleOptions("Default toast", NotificationType.Warning);
        options.Classes.Clear();
        Manager.Show(options);
    }

    private void ShowColorfulStyle(object? sender, Avalonia.Interactivity.RoutedEventArgs e) =>
        Manager.Show(StyleOptions("Colorful toast", NotificationType.Success, true));

    private NotificationOptions StyleOptions(string content, NotificationType type, bool colorful = false) => new()
    {
        Content = content,
        Type = type,
        IsColorful = colorful,
        OnClose = SetCloseReason
    };

    private void ShowClickable(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Manager.Show(
        new NotificationOptions
        {
            Content = "Click this toast to open the completed export.",
            Type = NotificationType.Success,
            Expiration = TimeSpan.FromSeconds(8),
            IsTouchClose = true,
            OnClick = () => LastInteraction = "Completed export requested",
            OnClose = SetCloseReason
        });

    private void ShowTimeout(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Manager.Show(
        new NotificationOptions
        {
            Content = "This toast closes automatically after two seconds.",
            Expiration = TimeSpan.FromSeconds(2),
            OnClose = SetCloseReason
        });

    private void ShowTracked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _trackedToast = new Toast(
            "Use Close tracked to close this specific toast.",
            NotificationType.Information,
            TimeSpan.Zero,
            showClose: true,
            onClose: SetCloseReason)
        {
            ShowIcon = true
        };
        Manager.Show(_trackedToast);
    }

    private void CloseTracked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_trackedToast is null)
        {
            LastInteraction = "No tracked toast";
            return;
        }

        Manager.Close(_trackedToast);
        _trackedToast = null;
    }

    private void ShowDisplaced(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        for (var i = 1; i <= Manager.MaxItems + 2; i++)
        {
            var index = i;
            Manager.Show(new NotificationOptions
            {
                Content = $"Queued toast {index}: older items are displaced at the visible limit.",
                Expiration = TimeSpan.Zero,
                OnClose = reason => LastCloseReason = $"#{index}: {reason}"
            });
        }
    }

    private void CloseAll(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Manager.CloseAll();
        _trackedToast = null;
    }

    private void SetCloseReason(MessageCloseReason reason) => LastCloseReason = reason.ToString();

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string ConfiguratorCode { get; } = """
                                              manager.Show(new NotificationOptions
                                              {
                                                  Content = ToastContent,
                                                  Type = SelectedType,
                                                  Expiration = TimeSpan.FromSeconds((double)ExpirationSeconds),
                                                  IsIconVisible = IsIconVisible,
                                                  IsCloseButtonVisible = IsCloseButtonVisible,
                                                  IsTouchClose = IsTouchClose,
                                                  IsColorful = IsColorful,
                                                  OperateButtons = actions,
                                                  OnClose = reason => LastCloseReason = reason.ToString()
                                              });
                                              """;

    public string PresetsCode { get; } = """
                                         manager.Show(new Toast(
                                             "Changes saved successfully.",
                                             NotificationType.Success,
                                             TimeSpan.FromSeconds(3),
                                             showClose: true));
                                         """;

    public string ActionsCode { get; } = """
                                         manager.Show(new NotificationOptions
                                         {
                                             Content = "The download is ready.",
                                             IsButtonsInline = true,
                                             OperateButtons = new ObservableCollection<OperateButtonEntry>
                                             {
                                                 new("Open", _ => OpenFile(), closeOnClick: true),
                                                 new("Folder", _ => OpenFolder())
                                             }
                                         });
                                         """;

    public string LifetimeCode { get; } = """
                                          var toast = new Toast(
                                              "Tracked toast",
                                              expiration: TimeSpan.Zero,
                                              onClose: reason => LastCloseReason = reason.ToString());

                                          manager.Show(toast);
                                          manager.Close(toast);
                                          manager.CloseAll();
                                          """;
}