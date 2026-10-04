using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace OvoUi.Controls;

public class OvoTitleBar : TemplatedControl
{
    private DateTime? _lastClickTime;
    private Button? _closeButton;
    private Button? _maximizeButton;
    private Button? _restoreButton;
    private Button? _minimizeButton;
    private Panel? _moveDragArea;
    private Window? _window;

    protected override Type StyleKeyOverride => typeof(OvoTitleBar);

    public static readonly StyledProperty<Thickness> ControlBtnMarginProperty =
        AvaloniaProperty.Register<OvoTitleBar, Thickness>(nameof(ControlBtnMargin), new Thickness(0, 0, 5, 0));

    public Thickness ControlBtnMargin
    {
        get => GetValue(ControlBtnMarginProperty);
        set => SetValue(ControlBtnMarginProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_closeButton is not null) _closeButton.Click -= CloseButton_Click;
        if (_maximizeButton is not null) _maximizeButton.Click -= MaximizeButton_Click;
        if (_restoreButton is not null) _restoreButton.Click -= MaximizeButton_Click;
        if (_minimizeButton is not null) _minimizeButton.Click -= MinimizeButton_Click;
        if (_moveDragArea is not null) _moveDragArea.PointerPressed -= MoveDragArea_PointerPressed;

        base.OnApplyTemplate(e);
        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        _maximizeButton = e.NameScope.Find<Button>("PART_MaximizeButton");
        _restoreButton = e.NameScope.Find<Button>("PART_RestoreButton");
        _minimizeButton = e.NameScope.Find<Button>("PART_MinimizeButton");
        _moveDragArea = e.NameScope.Find<Panel>("PART_MoveDragArea");

        if (_closeButton is not null) _closeButton.Click += CloseButton_Click;
        if (_maximizeButton is not null) _maximizeButton.Click += MaximizeButton_Click;
        if (_restoreButton is not null) _restoreButton.Click += MaximizeButton_Click;
        if (_minimizeButton is not null) _minimizeButton.Click += MinimizeButton_Click;
        if (_moveDragArea is not null) _moveDragArea.PointerPressed += MoveDragArea_PointerPressed;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _window = TopLevel.GetTopLevel(this) as Window;
        if (_window is not null) _window.PropertyChanged += Window_PropertyChanged;
        UpdateWindowState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_window is not null) _window.PropertyChanged -= Window_PropertyChanged;
        _window = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void Window_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty) UpdateWindowState();
    }

    private void UpdateWindowState()
    {
        PseudoClasses.Set(":maximized", _window?.WindowState == WindowState.Maximized);
    }

    private void MoveDragArea_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        if (sender is Panel control)
        {
            var window = TopLevel.GetTopLevel(control) as Window;
            window?.BeginMoveDrag(e);
        }

        if (IsMaxBtnShow && _lastClickTime.HasValue && (DateTime.Now - _lastClickTime.Value).TotalMilliseconds < 300)
        {
            _lastClickTime = null;
            if (TopLevel.GetTopLevel(this) is Window window)
                window.WindowState = window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
        }
        else
        {
            _lastClickTime = DateTime.Now;
        }

        e.Handled = true;
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (TopLevel.GetTopLevel(button) is not OvoWindow window) return;

        var handled = window.OnMinimize();
        if (handled) return;

        window.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (TopLevel.GetTopLevel(button) is not OvoWindow window) return;

        var handled = window.OnMaximize();
        if (handled) return;

        window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (TopLevel.GetTopLevel(button) is not OvoWindow window) return;

        var handled = window.OnClose();
        if (handled) return;

        window.Close();
    }

    #region Styled Properties

    public static readonly StyledProperty<object?> LeftContentProperty =
        AvaloniaProperty.Register<OvoTitleBar, object?>(nameof(LeftContent));

    public object? LeftContent
    {
        get => GetValue(LeftContentProperty);
        set => SetValue(LeftContentProperty, value);
    }

    public static readonly StyledProperty<double> TitleBarHeightProperty =
        AvaloniaProperty.Register<OvoTitleBar, double>(nameof(TitleBarHeight), 36);

    public double TitleBarHeight
    {
        get => GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }
    
    public static readonly StyledProperty<object?> RightContentProperty =
        AvaloniaProperty.Register<OvoTitleBar, object?>(nameof(RightContent));

    public object? RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    public static readonly StyledProperty<bool> IsCloseBtnShowProperty =
        AvaloniaProperty.Register<OvoTitleBar, bool>(nameof(IsCloseBtnShow), true);

    public bool IsCloseBtnShow
    {
        get => GetValue(IsCloseBtnShowProperty);
        set => SetValue(IsCloseBtnShowProperty, value);
    }

    public static readonly StyledProperty<bool> IsMaxBtnShowProperty =
        AvaloniaProperty.Register<OvoTitleBar, bool>(nameof(IsMaxBtnShow), true);

    public bool IsMaxBtnShow
    {
        get => GetValue(IsMaxBtnShowProperty);
        set => SetValue(IsMaxBtnShowProperty, value);
    }

    public static readonly StyledProperty<bool> IsMinBtnShowProperty =
        AvaloniaProperty.Register<OvoTitleBar, bool>(nameof(IsMinBtnShow), true);

    public bool IsMinBtnShow
    {
        get => GetValue(IsMinBtnShowProperty);
        set => SetValue(IsMinBtnShowProperty, value);
    }

    public static readonly StyledProperty<Geometry> MinimizeIconProperty =
        AvaloniaProperty.Register<OvoTitleBar, Geometry>(nameof(MinimizeIcon),
            StreamGeometry.Parse("M19 13H5a1 1 0 0 1 0-2h14a1 1 0 0 1 0 2z"));

    public Geometry MinimizeIcon
    {
        get => GetValue(MinimizeIconProperty);
        set => SetValue(MinimizeIconProperty, value);
    }

    public static readonly StyledProperty<Geometry> MaximizeIconProperty =
        AvaloniaProperty.Register<OvoTitleBar, Geometry>(nameof(MaximizeIcon),
            StreamGeometry.Parse("M18 21H6a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3zM6 5a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V6a1 1 0 0 0-1-1z"));

    public Geometry MaximizeIcon
    {
        get => GetValue(MaximizeIconProperty);
        set => SetValue(MaximizeIconProperty, value);
    }

    public static readonly StyledProperty<Geometry> RestoreIconProperty =
        AvaloniaProperty.Register<OvoTitleBar, Geometry>(nameof(RestoreIcon),
            StreamGeometry.Parse(
                "M18 21H6a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3zM6 5a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V6a1 1 0 0 0-1-1z"));

    public Geometry RestoreIcon
    {
        get => GetValue(RestoreIconProperty);
        set => SetValue(RestoreIconProperty, value);
    }

    public static readonly StyledProperty<Geometry> CloseIconProperty =
        AvaloniaProperty.Register<OvoTitleBar, Geometry>(nameof(CloseIcon),
            StreamGeometry.Parse("M13.41 12l4.3-4.29a1 1 0 1 0-1.42-1.42L12 10.59l-4.29-4.3a1 1 0 0 0-1.42 1.42l4.3 4.29-4.3 4.29a1 1 0 0 0 0 1.42 1 1 0 0 0 1.42 0l4.29-4.3 4.29 4.3a1 1 0 0 0 1.42 0 1 1 0 0 0 0-1.42z"));

    public Geometry CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    #endregion
    
}
