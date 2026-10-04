using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace OvoUi.Controls;

public partial class OvoTitleBar : UserControl
{
    private WindowState _windowStateBeforeFullScreen = WindowState.Normal;

    public static readonly AttachedProperty<Thickness> ControlButtonMarginProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, Thickness>(nameof(ControlButtonMargin),
            defaultValue: new Thickness(0, 0, 9, 0));

    public Thickness ControlButtonMargin
    {
        get => GetValue(ControlButtonMarginProperty);
        set => SetValue(ControlButtonMarginProperty, value);
    }

    public OvoTitleBar()
    {
        InitializeComponent();
        CloseButton.Click += CloseButton_Click;
        FullScreenButton.Click += FullScreenButton_Click;
        PinButton.Click += PinButton_Click;
        MaximizeButton.Click += MaximizeButton_Click;
        MinimizeButton.Click += MinimizeButton_Click;
        MoveDragArea.PointerPressed += MoveDragArea_PointerPressed;

        // if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //     AttachedToVisualTree += (_, _) =>
        //     {
        //         Debug.WriteLine("OvoTitleBar: AttachedToVisualTree event fired");
        //         EnableWindowsSnapLayout(MaximizeButton);
        //     };
    }

    private void MoveDragArea_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        if (TopLevel.GetTopLevel(this) is not Window window) return;

        if (window.WindowState == WindowState.FullScreen)
        {
            e.Handled = true;
            return;
        }

        if (e.ClickCount == 2 && IsMaximizeButtonVisible && window.CanResize)
            ToggleMaximize(window);
        else
            window.BeginMoveDrag(e);

        e.Handled = true;
    }

    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window window) return;
        if (window is OvoWindow ovoWindow && ovoWindow.OnMinimize()) return;

        window.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is Window window && window.CanResize)
            ToggleMaximize(window);
    }

    private void FullScreenButton_Click(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window window) return;
        if (window is OvoWindow ovoWindow)
        {
            if (!ovoWindow.OnFullScreen()) ovoWindow.ToggleFullScreen();
            return;
        }

        if (window.WindowState == WindowState.FullScreen)
        {
            window.WindowState = _windowStateBeforeFullScreen;
            return;
        }

        _windowStateBeforeFullScreen = window.WindowState == WindowState.Maximized
            ? WindowState.Maximized
            : WindowState.Normal;
        window.WindowState = WindowState.FullScreen;
    }

    internal void SetFullScreenMode(bool isFullScreen)
    {
        var role = isFullScreen
            ? WindowDecorationsElementRole.User
            : WindowDecorationsElementRole.TitleBar;

        Avalonia.Controls.Chrome.WindowDecorationProperties.SetElementRole(TitleBarPanel, role);
        Avalonia.Controls.Chrome.WindowDecorationProperties.SetElementRole(MoveDragArea, role);
        Avalonia.Controls.Chrome.WindowDecorationProperties.SetElementRole(TitleBarDockPanel, role);
        Avalonia.Controls.Chrome.WindowDecorationProperties.SetElementRole(TitleBarContentGrid, role);
    }

    private void PinButton_Click(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window window) return;
        if (window is OvoWindow ovoWindow && ovoWindow.OnPin()) return;

        window.Topmost = !window.Topmost;
    }

    private static void ToggleMaximize(Window window)
    {
        if (window is OvoWindow ovoWindow && ovoWindow.OnMaximize()) return;
        window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is not Window window) return;
        if (window is OvoWindow ovoWindow && ovoWindow.OnClose()) return;

        window.Close();
    }

    #region Styled Properties

    public static readonly AttachedProperty<object?> LeadingContentProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, object?>(nameof(LeadingContent));

    public object? LeadingContent
    {
        get => GetValue(LeadingContentProperty);
        set => SetValue(LeadingContentProperty, value);
    }

    public static readonly AttachedProperty<double> TitleBarHeightProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, double>(nameof(TitleBarHeight), 43,
            validate: value => double.IsFinite(value) && value >= 0);

    public double TitleBarHeight
    {
        get => GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }
    
    public static readonly AttachedProperty<Thickness> TitleBarMarginProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, Thickness>(nameof(TitleBarMargin));

    public Thickness TitleBarMargin
    {
        get => GetValue(TitleBarMarginProperty);
        set => SetValue(TitleBarMarginProperty, value);
    }
    
    public static readonly AttachedProperty<Thickness> TitleBarPaddingProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, Thickness>(nameof(TitleBarPadding));
    
    public Thickness TitleBarPadding
    {
        get => GetValue(TitleBarPaddingProperty);
        set => SetValue(TitleBarPaddingProperty, value);
    }

    public static readonly AttachedProperty<object?> TrailingContentProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, object?>(nameof(TrailingContent));

    public object? TrailingContent
    {
        get => GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }

    public static readonly AttachedProperty<bool> IsCloseButtonVisibleProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, bool>(nameof(IsCloseButtonVisible), true);

    public bool IsCloseButtonVisible
    {
        get => GetValue(IsCloseButtonVisibleProperty);
        set => SetValue(IsCloseButtonVisibleProperty, value);
    }

    public static readonly AttachedProperty<bool> IsMaximizeButtonVisibleProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, bool>(nameof(IsMaximizeButtonVisible), true);

    public bool IsMaximizeButtonVisible
    {
        get => GetValue(IsMaximizeButtonVisibleProperty);
        set => SetValue(IsMaximizeButtonVisibleProperty, value);
    }

    public static readonly AttachedProperty<bool> IsMinimizeButtonVisibleProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, bool>(nameof(IsMinimizeButtonVisible), true);

    public bool IsMinimizeButtonVisible
    {
        get => GetValue(IsMinimizeButtonVisibleProperty);
        set => SetValue(IsMinimizeButtonVisibleProperty, value);
    }

    public static readonly AttachedProperty<bool> IsFullScreenButtonVisibleProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, bool>(nameof(IsFullScreenButtonVisible), true);

    public bool IsFullScreenButtonVisible
    {
        get => GetValue(IsFullScreenButtonVisibleProperty);
        set => SetValue(IsFullScreenButtonVisibleProperty, value);
    }

    public static readonly AttachedProperty<bool> IsPinButtonVisibleProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, bool>(nameof(IsPinButtonVisible), true);

    public bool IsPinButtonVisible
    {
        get => GetValue(IsPinButtonVisibleProperty);
        set => SetValue(IsPinButtonVisibleProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> MinimizeButtonIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(MinimizeButtonIcon),
            StreamGeometry.Parse(
                "F1 M3 8C3 7.58579 3.33579 7.25 3.75 7.25L12.25 7.25C12.6642 7.25 13 7.58579 13 8C13 8.41421 12.6642 8.75 12.25 8.75L3.75 8.75C3.33579 8.75 3 8.41421 3 8Z"));

    public StreamGeometry MinimizeButtonIcon
    {
        get => GetValue(MinimizeButtonIconProperty);
        set => SetValue(MinimizeButtonIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> MaximizeButtonIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(MaximizeButtonIcon),
            StreamGeometry.Parse(
                "F1 M8.66 10.59L1.93 10.59C0.86 10.59 0 9.72 0 8.66L0 1.93C0 0.87 0.87 0 1.93 0L8.66 0C9.73 0 10.59 0.87 10.59 1.93L10.59 8.66C10.59 9.73 9.72 10.59 8.66 10.59ZM1.93 1.4C1.64 1.4 1.4 1.64 1.4 1.93L1.4 8.66C1.4 8.95 1.64 9.19 1.93 9.19L8.66 9.19C8.95 9.19 9.19 8.95 9.19 8.66L9.19 1.93C9.19 1.64 8.95 1.4 8.66 1.4L1.93 1.4Z"));

    public StreamGeometry MaximizeButtonIcon
    {
        get => GetValue(MaximizeButtonIconProperty);
        set => SetValue(MaximizeButtonIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> RestoreButtonIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(RestoreButtonIcon),
            StreamGeometry.Parse(
                "F1 M11.29 0L4.56 0C3.68 0 2.94 0.6 2.71 1.41L4.5 1.41C4.5 1.41 4.54 1.4 4.56 1.4L11.29 1.4C11.58 1.4 11.82 1.64 11.82 1.93L11.82 10.51C12.62 10.28 13.22 9.54 13.22 8.66L13.22 1.93C13.22 0.87 12.36 0 11.29 0Z F1 M8.66 2.63L1.93 2.63C0.87 2.63 0 3.49 0 4.56L0 11.29C0 12.36 0.87 13.22 1.93 13.22L8.66 13.22C9.73 13.22 10.59 12.35 10.59 11.29L10.59 4.56C10.59 3.49 9.72 2.63 8.66 2.63ZM9.19 11.29C9.19 11.58 8.95 11.82 8.66 11.82L1.93 11.82C1.64 11.82 1.4 11.58 1.4 11.29L1.4 4.56C1.4 4.27 1.64 4.03 1.93 4.03L8.66 4.03C8.95 4.03 9.19 4.27 9.19 4.56L9.19 11.29L9.19 11.29Z"));

    public StreamGeometry RestoreButtonIcon
    {
        get => GetValue(RestoreButtonIconProperty);
        set => SetValue(RestoreButtonIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> CloseButtonIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(CloseButtonIcon),
            StreamGeometry.Parse(
                "F1 M2.39705 2.55379L2.46967 2.46967C2.73594 2.2034 3.1526 2.1792 3.44621 2.39705L3.53033 2.46967L8 6.939L12.4697 2.46967C12.7626 2.17678 13.2374 2.17678 13.5303 2.46967C13.8232 2.76256 13.8232 3.23744 13.5303 3.53033L9.061 8L13.5303 12.4697C13.7966 12.7359 13.8208 13.1526 13.6029 13.4462L13.5303 13.5303C13.2641 13.7966 12.8474 13.8208 12.5538 13.6029L12.4697 13.5303L8 9.061L3.53033 13.5303C3.23744 13.8232 2.76256 13.8232 2.46967 13.5303C2.17678 13.2374 2.17678 12.7626 2.46967 12.4697L6.939 8L2.46967 3.53033C2.2034 3.26406 2.1792 2.8474 2.39705 2.55379L2.46967 2.46967L2.39705 2.55379Z"));

    public StreamGeometry CloseButtonIcon
    {
        get => GetValue(CloseButtonIconProperty);
        set => SetValue(CloseButtonIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> FullScreenButtonIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(FullScreenButtonIcon),
            StreamGeometry.Parse(
                "M5,5H10V7H7V10H5V5M14,5H19V10H17V7H14V5M17,14H19V19H14V17H17V14M10,17V19H5V14H7V17H10Z"));

    public StreamGeometry FullScreenButtonIcon
    {
        get => GetValue(FullScreenButtonIconProperty);
        set => SetValue(FullScreenButtonIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> ExitFullScreenButtonIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(ExitFullScreenButtonIcon),
            StreamGeometry.Parse(
                "M14,14H19V16H16V19H14V14M5,14H10V19H8V16H5V14M8,5H10V10H5V8H8V5M19,8V10H14V5H16V8H19Z"));

    public StreamGeometry ExitFullScreenButtonIcon
    {
        get => GetValue(ExitFullScreenButtonIconProperty);
        set => SetValue(ExitFullScreenButtonIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> PinButtonIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(PinButtonIcon),
            StreamGeometry.Parse(
                "M16,12V4H17V2H7V4H8V12L6,14V16H11.2V22H12.8V16H18V14L16,12M8.8,14L10,12.8V4H14V12.8L15.2,14H8.8Z"));

    public StreamGeometry PinButtonIcon
    {
        get => GetValue(PinButtonIconProperty);
        set => SetValue(PinButtonIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> UnpinButtonIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(UnpinButtonIcon),
            StreamGeometry.Parse(
                "M8,6.2V4H7V2H17V4H16V12L18,14V16H17.8L14,12.2V4H10V8.2L8,6.2M20,20.7L18.7,22L12.8,16.1V22H11.2V16H6V14L8,12V11.3L2,5.3L3.3,4L20,20.7M8.8,14H10.6L9.7,13.1L8.8,14Z"));

    public StreamGeometry UnpinButtonIcon
    {
        get => GetValue(UnpinButtonIconProperty);
        set => SetValue(UnpinButtonIconProperty, value);
    }

    #endregion

    #region Attached Property Accessors

    public static Thickness GetControlButtonMargin(Control control) => control.GetValue(ControlButtonMarginProperty);

    public static void SetControlButtonMargin(Control control, Thickness value) =>
        control.SetValue(ControlButtonMarginProperty, value);

    public static double GetTitleBarHeight(Control control) => control.GetValue(TitleBarHeightProperty);

    public static void SetTitleBarHeight(Control control, double value) =>
        control.SetValue(TitleBarHeightProperty, value);

    public static object? GetLeadingContent(Control control) => control.GetValue(LeadingContentProperty);

    public static void SetLeadingContent(Control control, object? value) =>
        control.SetValue(LeadingContentProperty, value);

    public static object? GetTrailingContent(Control control) => control.GetValue(TrailingContentProperty);

    public static void SetTrailingContent(Control control, object? value) =>
        control.SetValue(TrailingContentProperty, value);

    public static bool GetIsCloseButtonVisible(Control control) => control.GetValue(IsCloseButtonVisibleProperty);

    public static void SetIsCloseButtonVisible(Control control, bool value) =>
        control.SetValue(IsCloseButtonVisibleProperty, value);

    public static bool GetIsMaximizeButtonVisible(Control control) => control.GetValue(IsMaximizeButtonVisibleProperty);

    public static void SetIsMaximizeButtonVisible(Control control, bool value) =>
        control.SetValue(IsMaximizeButtonVisibleProperty, value);

    public static bool GetIsMinimizeButtonVisible(Control control) => control.GetValue(IsMinimizeButtonVisibleProperty);

    public static void SetIsMinimizeButtonVisible(Control control, bool value) =>
        control.SetValue(IsMinimizeButtonVisibleProperty, value);

    public static bool GetIsFullScreenButtonVisible(Control control) =>
        control.GetValue(IsFullScreenButtonVisibleProperty);

    public static void SetIsFullScreenButtonVisible(Control control, bool value) =>
        control.SetValue(IsFullScreenButtonVisibleProperty, value);

    public static bool GetIsPinButtonVisible(Control control) => control.GetValue(IsPinButtonVisibleProperty);

    public static void SetIsPinButtonVisible(Control control, bool value) =>
        control.SetValue(IsPinButtonVisibleProperty, value);

    public static StreamGeometry GetMinimizeButtonIcon(Control control) => control.GetValue(MinimizeButtonIconProperty);

    public static void SetMinimizeButtonIcon(Control control, StreamGeometry value) =>
        control.SetValue(MinimizeButtonIconProperty, value);

    public static StreamGeometry GetMaximizeButtonIcon(Control control) => control.GetValue(MaximizeButtonIconProperty);

    public static void SetMaximizeButtonIcon(Control control, StreamGeometry value) =>
        control.SetValue(MaximizeButtonIconProperty, value);

    public static StreamGeometry GetRestoreButtonIcon(Control control) => control.GetValue(RestoreButtonIconProperty);

    public static void SetRestoreButtonIcon(Control control, StreamGeometry value) =>
        control.SetValue(RestoreButtonIconProperty, value);

    public static StreamGeometry GetCloseButtonIcon(Control control) => control.GetValue(CloseButtonIconProperty);

    public static void SetCloseButtonIcon(Control control, StreamGeometry value) =>
        control.SetValue(CloseButtonIconProperty, value);

    public static StreamGeometry GetFullScreenButtonIcon(Control control) =>
        control.GetValue(FullScreenButtonIconProperty);

    public static void SetFullScreenButtonIcon(Control control, StreamGeometry value) =>
        control.SetValue(FullScreenButtonIconProperty, value);

    public static StreamGeometry GetExitFullScreenButtonIcon(Control control) =>
        control.GetValue(ExitFullScreenButtonIconProperty);

    public static void SetExitFullScreenButtonIcon(Control control, StreamGeometry value) =>
        control.SetValue(ExitFullScreenButtonIconProperty, value);

    public static StreamGeometry GetPinButtonIcon(Control control) => control.GetValue(PinButtonIconProperty);

    public static void SetPinButtonIcon(Control control, StreamGeometry value) =>
        control.SetValue(PinButtonIconProperty, value);

    public static StreamGeometry GetUnpinButtonIcon(Control control) => control.GetValue(UnpinButtonIconProperty);

    public static void SetUnpinButtonIcon(Control control, StreamGeometry value) =>
        control.SetValue(UnpinButtonIconProperty, value);

    #endregion

    #region Windows Snap Layout Support

    // [DllImport("user32.dll")]
    // private static extern short GetAsyncKeyState(int vKey);
    //
    // private static bool IsMouseDown()
    // {
    //     if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    //         return false;
    //
    //     const int VK_LBUTTON = 1;
    //     return (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
    // }
    //
    // private void EnableWindowsSnapLayout(Button maximizeButton)
    // {
    //     if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    //         return;
    //
    //     const int HTCLIENT = 1;
    //     const int HTMAXBUTTON = 9;
    //     const uint WM_NCHITTEST = 0x0084;
    //
    //     var pointerOnButton = false;
    //     var pointerOverSetter = typeof(Button).GetProperty(nameof(IsPointerOver));
    //     if (pointerOverSetter is null)
    //     {
    //         Debug.WriteLine("OvoTitleBar: IsPointerOver property not found");
    //         return;
    //     }
    //
    //     var window = TopLevel.GetTopLevel(this) as Window;
    //     if (window == null)
    //     {
    //         Debug.WriteLine("OvoTitleBar: Window not found");
    //         return;
    //     }
    //
    //     Debug.WriteLine("OvoTitleBar: Enabling Snap Layout for button");
    //
    //     try
    //     {
    //         _wndProcHookCallback = ProcHookCallback;
    //         Win32Properties.AddWndProcHookCallback(window, _wndProcHookCallback);
    //
    //         Debug.WriteLine("OvoTitleBar: Win32 hook successfully registered");
    //     }
    //     catch (Exception ex)
    //     {
    //         Debug.WriteLine($"OvoTitleBar: Failed to enable Windows Snap Layout: {ex.Message}");
    //         Debug.WriteLine($"OvoTitleBar: Stack trace: {ex.StackTrace}");
    //     }
    //
    //     return;
    //
    //     nint ProcHookCallback(nint hWnd, uint msg, nint wParam, nint lParam, ref bool handled)
    //     {
    //         if (msg == WM_NCHITTEST)
    //         {
    //             if (!maximizeButton.IsVisible)
    //                 return 0;
    //
    //             var point = new PixelPoint(
    //                 (short)(ToInt32(lParam) & 0xffff),
    //                 (short)(ToInt32(lParam) >> 16)
    //             );
    //
    //             var buttonSize = maximizeButton.DesiredSize;
    //             var buttonLeftTop = maximizeButton.PointToScreen(new Point(0, 0));
    //
    //             var scaling = window.RenderScaling;
    //             var x = (point.X - buttonLeftTop.X) / scaling;
    //             var y = (point.Y - buttonLeftTop.Y) / scaling;
    //
    //             var isInButton = new Rect(default, buttonSize).Contains(new Point(x, y));
    //
    //             if (isInButton)
    //             {
    //                 handled = true;
    //
    //                 if (!pointerOnButton)
    //                 {
    //                     pointerOnButton = true;
    //                     pointerOverSetter.SetValue(maximizeButton, true);
    //                     // Debug.WriteLine("OvoTitleBar: Pointer entered maximize button");
    //                 }
    //
    //                 var result = IsMouseDown() ? HTCLIENT : HTMAXBUTTON;
    //                 // Debug.WriteLine($"OvoTitleBar: Returning {(result == HTMAXBUTTON ? "HTMAXBUTTON" : "HTCLIENT")}");
    //                 return result;
    //             }
    //
    //             if (!pointerOnButton) return 0;
    //             pointerOnButton = false;
    //             pointerOverSetter.SetValue(maximizeButton, false);
    //             // Debug.WriteLine("OvoTitleBar: Pointer left maximize button");
    //         }
    //
    //         return 0;
    //     }
    //
    //     static int ToInt32(IntPtr ptr)
    //     {
    //         return IntPtr.Size == 4 ? ptr.ToInt32() : (int)(ptr.ToInt64() & 0xffffffff);
    //     }
    // }

    #endregion
}
