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
    public static readonly AttachedProperty<Thickness> ControlBtnMarginProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, Thickness>(nameof(ControlBtnMargin), new Thickness(0, 0, 5, 0));

    public Thickness ControlBtnMargin
    {
        get => GetValue(ControlBtnMarginProperty);
        set => SetValue(ControlBtnMarginProperty, value);
    }

    public OvoTitleBar()
    {
        InitializeComponent();
        CloseButton.Click += CloseButton_Click;
        MaximizeButton.Click += MaximizeButton_Click;
        RestoreButton.Click += MaximizeButton_Click;
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

        if (e.ClickCount == 2 && IsMaxBtnShow && window.CanResize)
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

    public static readonly AttachedProperty<object?> LeftContentProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, object?>(nameof(LeftContent));

    public object? LeftContent
    {
        get => GetValue(LeftContentProperty);
        set => SetValue(LeftContentProperty, value);
    }

    public static readonly AttachedProperty<double> TitleBarHeightProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, double>(nameof(TitleBarHeight), 40,
            validate: value => double.IsFinite(value) && value >= 0);

    public double TitleBarHeight
    {
        get => GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }
    
    public static readonly AttachedProperty<object?> RightContentProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, object?>(nameof(RightContent));

    public object? RightContent
    {
        get => GetValue(RightContentProperty);
        set => SetValue(RightContentProperty, value);
    }

    public static readonly AttachedProperty<bool> IsCloseBtnShowProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, bool>(nameof(IsCloseBtnShow), true);

    public bool IsCloseBtnShow
    {
        get => GetValue(IsCloseBtnShowProperty);
        set => SetValue(IsCloseBtnShowProperty, value);
    }

    public static readonly AttachedProperty<bool> IsMaxBtnShowProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, bool>(nameof(IsMaxBtnShow), true);

    public bool IsMaxBtnShow
    {
        get => GetValue(IsMaxBtnShowProperty);
        set => SetValue(IsMaxBtnShowProperty, value);
    }

    public static readonly AttachedProperty<bool> IsMinBtnShowProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, bool>(nameof(IsMinBtnShow), true);

    public bool IsMinBtnShow
    {
        get => GetValue(IsMinBtnShowProperty);
        set => SetValue(IsMinBtnShowProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> MinimizeIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(MinimizeIcon),
            StreamGeometry.Parse("M19 13H5a1 1 0 0 1 0-2h14a1 1 0 0 1 0 2z"));

    public StreamGeometry MinimizeIcon
    {
        get => GetValue(MinimizeIconProperty);
        set => SetValue(MinimizeIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> MaximizeIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(MaximizeIcon),
            StreamGeometry.Parse("M18 21H6a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3zM6 5a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V6a1 1 0 0 0-1-1z"));

    public StreamGeometry MaximizeIcon
    {
        get => GetValue(MaximizeIconProperty);
        set => SetValue(MaximizeIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> RestoreIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(RestoreIcon),
            StreamGeometry.Parse(
                "M18 21H6a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3zM6 5a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V6a1 1 0 0 0-1-1z"));

    public StreamGeometry RestoreIcon
    {
        get => GetValue(RestoreIconProperty);
        set => SetValue(RestoreIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> CloseIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(CloseIcon),
            StreamGeometry.Parse("M13.41 12l4.3-4.29a1 1 0 1 0-1.42-1.42L12 10.59l-4.29-4.3a1 1 0 0 0-1.42 1.42l4.3 4.29-4.3 4.29a1 1 0 0 0 0 1.42 1 1 0 0 0 1.42 0l4.29-4.3 4.29 4.3a1 1 0 0 0 1.42 0 1 1 0 0 0 0-1.42z"));

    public StreamGeometry CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    #endregion

    #region Attached Property Accessors

    public static Thickness GetControlBtnMargin(Control control) => control.GetValue(ControlBtnMarginProperty);
    public static void SetControlBtnMargin(Control control, Thickness value) => control.SetValue(ControlBtnMarginProperty, value);
    public static double GetTitleBarHeight(Control control) => control.GetValue(TitleBarHeightProperty);
    public static void SetTitleBarHeight(Control control, double value) => control.SetValue(TitleBarHeightProperty, value);
    public static object? GetLeftContent(Control control) => control.GetValue(LeftContentProperty);
    public static void SetLeftContent(Control control, object? value) => control.SetValue(LeftContentProperty, value);
    public static object? GetRightContent(Control control) => control.GetValue(RightContentProperty);
    public static void SetRightContent(Control control, object? value) => control.SetValue(RightContentProperty, value);
    public static bool GetIsCloseBtnShow(Control control) => control.GetValue(IsCloseBtnShowProperty);
    public static void SetIsCloseBtnShow(Control control, bool value) => control.SetValue(IsCloseBtnShowProperty, value);
    public static bool GetIsMaxBtnShow(Control control) => control.GetValue(IsMaxBtnShowProperty);
    public static void SetIsMaxBtnShow(Control control, bool value) => control.SetValue(IsMaxBtnShowProperty, value);
    public static bool GetIsMinBtnShow(Control control) => control.GetValue(IsMinBtnShowProperty);
    public static void SetIsMinBtnShow(Control control, bool value) => control.SetValue(IsMinBtnShowProperty, value);
    public static StreamGeometry GetMinimizeIcon(Control control) => control.GetValue(MinimizeIconProperty);
    public static void SetMinimizeIcon(Control control, StreamGeometry value) => control.SetValue(MinimizeIconProperty, value);
    public static StreamGeometry GetMaximizeIcon(Control control) => control.GetValue(MaximizeIconProperty);
    public static void SetMaximizeIcon(Control control, StreamGeometry value) => control.SetValue(MaximizeIconProperty, value);
    public static StreamGeometry GetRestoreIcon(Control control) => control.GetValue(RestoreIconProperty);
    public static void SetRestoreIcon(Control control, StreamGeometry value) => control.SetValue(RestoreIconProperty, value);
    public static StreamGeometry GetCloseIcon(Control control) => control.GetValue(CloseIconProperty);
    public static void SetCloseIcon(Control control, StreamGeometry value) => control.SetValue(CloseIconProperty, value);

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
