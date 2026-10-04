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
    public static readonly AttachedProperty<Thickness> ControlButtonMarginProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, Thickness>(nameof(ControlButtonMargin), new Thickness(0, 0, 5, 0));

    public Thickness ControlButtonMargin
    {
        get => GetValue(ControlButtonMarginProperty);
        set => SetValue(ControlButtonMarginProperty, value);
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
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, double>(nameof(TitleBarHeight), 36,
            validate: value => double.IsFinite(value) && value >= 0);

    public double TitleBarHeight
    {
        get => GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
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

    public static readonly AttachedProperty<StreamGeometry> MinimizeButtonIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(MinimizeButtonIcon),
            StreamGeometry.Parse("M19 13H5a1 1 0 0 1 0-2h14a1 1 0 0 1 0 2z"));

    public StreamGeometry MinimizeButtonIcon
    {
        get => GetValue(MinimizeButtonIconProperty);
        set => SetValue(MinimizeButtonIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> MaximizeButtonIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(MaximizeButtonIcon),
            StreamGeometry.Parse("M18 21H6a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3zM6 5a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V6a1 1 0 0 0-1-1z"));

    public StreamGeometry MaximizeButtonIcon
    {
        get => GetValue(MaximizeButtonIconProperty);
        set => SetValue(MaximizeButtonIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> RestoreButtonIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(RestoreButtonIcon),
            StreamGeometry.Parse(
                "M18 21H6a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3zM6 5a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V6a1 1 0 0 0-1-1z"));

    public StreamGeometry RestoreButtonIcon
    {
        get => GetValue(RestoreButtonIconProperty);
        set => SetValue(RestoreButtonIconProperty, value);
    }

    public static readonly AttachedProperty<StreamGeometry> CloseButtonIconProperty =
        AvaloniaProperty.RegisterAttached<OvoTitleBar, Control, StreamGeometry>(nameof(CloseButtonIcon),
            StreamGeometry.Parse("M13.41 12l4.3-4.29a1 1 0 1 0-1.42-1.42L12 10.59l-4.29-4.3a1 1 0 0 0-1.42 1.42l4.3 4.29-4.3 4.29a1 1 0 0 0 0 1.42 1 1 0 0 0 1.42 0l4.29-4.3 4.29 4.3a1 1 0 0 0 1.42 0 1 1 0 0 0 0-1.42z"));

    public StreamGeometry CloseButtonIcon
    {
        get => GetValue(CloseButtonIconProperty);
        set => SetValue(CloseButtonIconProperty, value);
    }

    #endregion

    #region Attached Property Accessors

    public static Thickness GetControlButtonMargin(Control control) => control.GetValue(ControlButtonMarginProperty);
    public static void SetControlButtonMargin(Control control, Thickness value) => control.SetValue(ControlButtonMarginProperty, value);
    public static double GetTitleBarHeight(Control control) => control.GetValue(TitleBarHeightProperty);
    public static void SetTitleBarHeight(Control control, double value) => control.SetValue(TitleBarHeightProperty, value);
    public static object? GetLeadingContent(Control control) => control.GetValue(LeadingContentProperty);
    public static void SetLeadingContent(Control control, object? value) => control.SetValue(LeadingContentProperty, value);
    public static object? GetTrailingContent(Control control) => control.GetValue(TrailingContentProperty);
    public static void SetTrailingContent(Control control, object? value) => control.SetValue(TrailingContentProperty, value);
    public static bool GetIsCloseButtonVisible(Control control) => control.GetValue(IsCloseButtonVisibleProperty);
    public static void SetIsCloseButtonVisible(Control control, bool value) => control.SetValue(IsCloseButtonVisibleProperty, value);
    public static bool GetIsMaximizeButtonVisible(Control control) => control.GetValue(IsMaximizeButtonVisibleProperty);
    public static void SetIsMaximizeButtonVisible(Control control, bool value) => control.SetValue(IsMaximizeButtonVisibleProperty, value);
    public static bool GetIsMinimizeButtonVisible(Control control) => control.GetValue(IsMinimizeButtonVisibleProperty);
    public static void SetIsMinimizeButtonVisible(Control control, bool value) => control.SetValue(IsMinimizeButtonVisibleProperty, value);
    public static StreamGeometry GetMinimizeButtonIcon(Control control) => control.GetValue(MinimizeButtonIconProperty);
    public static void SetMinimizeButtonIcon(Control control, StreamGeometry value) => control.SetValue(MinimizeButtonIconProperty, value);
    public static StreamGeometry GetMaximizeButtonIcon(Control control) => control.GetValue(MaximizeButtonIconProperty);
    public static void SetMaximizeButtonIcon(Control control, StreamGeometry value) => control.SetValue(MaximizeButtonIconProperty, value);
    public static StreamGeometry GetRestoreButtonIcon(Control control) => control.GetValue(RestoreButtonIconProperty);
    public static void SetRestoreButtonIcon(Control control, StreamGeometry value) => control.SetValue(RestoreButtonIconProperty, value);
    public static StreamGeometry GetCloseButtonIcon(Control control) => control.GetValue(CloseButtonIconProperty);
    public static void SetCloseButtonIcon(Control control, StreamGeometry value) => control.SetValue(CloseButtonIconProperty, value);

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
