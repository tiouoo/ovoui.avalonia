using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using OvoUi.Common.Classes;
using OvoUi.Common.Interfaces;

namespace OvoUi.Controls;

[PseudoClasses(PC_Information, PC_Success, PC_Warning, PC_Error)]
public abstract class OvoMessageCard : ContentControl
{
    public const string PC_Information = ":information";
    public const string PC_Success = ":success";
    public const string PC_Warning = ":warning";
    public const string PC_Error = ":error";

    public static readonly DirectProperty<OvoMessageCard, bool> IsClosingProperty =
        AvaloniaProperty.RegisterDirect<OvoMessageCard, bool>(nameof(IsClosing), o => o.IsClosing);

    public static readonly StyledProperty<bool> IsClosedProperty =
        AvaloniaProperty.Register<OvoMessageCard, bool>(nameof(IsClosed));

    public static readonly StyledProperty<NotificationType> NotificationTypeProperty =
        AvaloniaProperty.Register<OvoMessageCard, NotificationType>(nameof(NotificationType));

    public static readonly StyledProperty<bool> ShowIconProperty =
        AvaloniaProperty.Register<OvoMessageCard, bool>(nameof(ShowIcon), true);

    public static readonly StyledProperty<IList<OperateButtonEntry>?> OperateButtonsProperty =
        AvaloniaProperty.Register<OvoMessageCard, IList<OperateButtonEntry>?>(nameof(OperateButtons));

    public static readonly StyledProperty<bool> IsButtonsInlineProperty =
        AvaloniaProperty.Register<OvoMessageCard, bool>(nameof(IsButtonsInline));

    public static readonly StyledProperty<NotificationEntry?> NotificationEntryProperty =
        AvaloniaProperty.Register<OvoMessageCard, NotificationEntry?>(nameof(NotificationEntry));

    public static readonly StyledProperty<bool> ShowCollapseButtonProperty =
        AvaloniaProperty.Register<OvoMessageCard, bool>(nameof(ShowCollapseButton));

    public static readonly StyledProperty<bool> ShowRemoveButtonProperty =
        AvaloniaProperty.Register<OvoMessageCard, bool>(nameof(ShowRemoveButton), true);

    public static readonly StyledProperty<Action?> OnRemoveProperty =
        AvaloniaProperty.Register<OvoMessageCard, Action?>(nameof(OnRemove));

    public static readonly RoutedEvent<MessageClosedEventArgs> MessageClosedEvent =
        RoutedEvent.Register<OvoMessageCard, MessageClosedEventArgs>(nameof(MessageClosed), RoutingStrategies.Bubble);

    public static readonly AttachedProperty<bool> CloseOnClickProperty =
        AvaloniaProperty.RegisterAttached<OvoMessageCard, Button, bool>("CloseOnClick");


    static OvoMessageCard()
    {
        CloseOnClickProperty.Changed.AddClassHandler<Button>(OnCloseOnClickPropertyChanged);
    }

    public OvoMessageCard()
    {
        UpdateNotificationType();
    }

    /// <summary>
    /// Gets the reason the message was closed.
    /// </summary>
    private MessageCloseReason _closeReason = MessageCloseReason.UserAction;

    public bool IsClosing
    {
        get;
        private set => SetAndRaise(IsClosingProperty, ref field, value);
    }

    public bool IsClosed
    {
        get => GetValue(IsClosedProperty);
        set => SetValue(IsClosedProperty, value);
    }

    public Action? OnRemove
    {
        get => GetValue(OnRemoveProperty);
        set => SetValue(OnRemoveProperty, value);
    }

    public NotificationType NotificationType
    {
        get => GetValue(NotificationTypeProperty);
        set => SetValue(NotificationTypeProperty, value);
    }

    public bool ShowIcon
    {
        get => GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public IList<OperateButtonEntry>? OperateButtons
    {
        get => GetValue(OperateButtonsProperty);
        set => SetValue(OperateButtonsProperty, value);
    }

    public bool IsButtonsInline
    {
        get => GetValue(IsButtonsInlineProperty);
        set => SetValue(IsButtonsInlineProperty, value);
    }

    public bool ShowCollapseButton
    {
        get => GetValue(ShowCollapseButtonProperty);
        set => SetValue(ShowCollapseButtonProperty, value);
    }

    public bool ShowRemoveButton
    {
        get => GetValue(ShowRemoveButtonProperty);
        set => SetValue(ShowRemoveButtonProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // 处理内联模式的按钮
        var collapseButton = e.NameScope.Find<Button>("PART_CollapseButton");
        if (collapseButton != null) collapseButton.Click += OnCollapseButtonClick;

        var removeButton = e.NameScope.Find<Button>("PART_RemoveButton");
        if (removeButton != null) removeButton.Click += OnRemoveButtonClick;

        // 处理非内联模式的按钮
        var collapseButton2 = e.NameScope.Find<Button>("PART_CollapseButton2");
        if (collapseButton2 != null) collapseButton2.Click += OnCollapseButtonClick;

        var removeButton2 = e.NameScope.Find<Button>("PART_RemoveButton2");
        if (removeButton2 != null) removeButton2.Click += OnRemoveButtonClick;
    }

    private void OnCollapseButtonClick(object? sender, RoutedEventArgs e)
    {
        CloseWithoutRemovingFromList();
    }

    private void OnRemoveButtonClick(object? sender, RoutedEventArgs e)
    {
        RemoveAndDelete();
    }

    public event EventHandler<MessageClosedEventArgs>? MessageClosed
    {
        add => AddHandler(MessageClosedEvent, value);
        remove => RemoveHandler(MessageClosedEvent, value);
    }

    public static bool GetCloseOnClick(Button obj)
    {
        _ = obj ?? throw new ArgumentNullException(nameof(obj));
        return obj.GetValue(CloseOnClickProperty);
    }

    public static void SetCloseOnClick(Button obj, bool value)
    {
        _ = obj ?? throw new ArgumentNullException(nameof(obj));
        obj.SetValue(CloseOnClickProperty, value);
    }

    private static void OnCloseOnClickPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
    {
        var button = (Button)d;
        var value = (bool)e.NewValue!;
        if (value)
            button.Click += Button_Click;
        else
            button.Click -= Button_Click;
    }

    private static void Button_Click(object? sender, RoutedEventArgs e)
    {
        var btn = sender as ILogical;
        var message = btn?.GetLogicalAncestors().OfType<OvoMessageCard>().FirstOrDefault();
        message?.Close(MessageCloseReason.UserAction);
    }

    /// <summary>
    /// Closes the <see cref="OvoMessageCard"/> with the specified <see cref="MessageCloseReason"/>.
    /// </summary>
    /// <param name="reason">The reason the message is being closed.</param>
    public void Close(MessageCloseReason reason = MessageCloseReason.UserAction)
    {
        if (IsClosing) return;

        _closeReason = reason;
        IsClosing = true;
        IsClosed = true;

        OnRemove?.Invoke();
    }

    /// <summary>
    ///     关闭 Toast 卡片但不从通知列表中移除
    /// </summary>
    public void CloseWithoutRemovingFromList(MessageCloseReason reason = MessageCloseReason.UserAction)
    {
        if (IsClosing) return;

        _closeReason = reason;
        IsClosing = true;
        IsClosed = true;
        // 不调用 NotificationEntry?.Remove()
    }

    /// <summary>
    ///     移除通知（从列表中移除并删除）
    /// </summary>
    public void RemoveAndDelete(MessageCloseReason reason = MessageCloseReason.UserAction)
    {
        if (IsClosing) return;

        _closeReason = reason;
        IsClosing = true;
        IsClosed = true;

        OnRemove?.Invoke();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == ContentProperty && e.NewValue is IMessage message)
            SetValue(NotificationTypeProperty, message.Type);

        if (e.Property == NotificationTypeProperty) UpdateNotificationType();

        if (e.Property == IsClosedProperty)
        {
            if (!IsClosing && !IsClosed) return;

            RaiseEvent(new MessageClosedEventArgs(_closeReason) { RoutedEvent = MessageClosedEvent });
        }
    }

    private void UpdateNotificationType()
    {
        switch (NotificationType)
        {
            case NotificationType.Error:
                PseudoClasses.Add(PC_Error);
                break;

            case NotificationType.Information:
                PseudoClasses.Add(PC_Information);
                break;

            case NotificationType.Success:
                PseudoClasses.Add(PC_Success);
                break;

            case NotificationType.Warning:
                PseudoClasses.Add(PC_Warning);
                break;
        }
    }
}