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
    private DateTime? _lastClickTime;

    public static readonly StyledProperty<Thickness> ControlBtnMarginProperty =
        AvaloniaProperty.Register<OvoTitleBar, Thickness>(nameof(ControlBtnMargin), new Thickness(0, 0, 5, 0));

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
        //         Debug.WriteLine("TioTitleBar: AttachedToVisualTree event fired");
        //         EnableWindowsSnapLayout(MaximizeButton);
        //     };
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

        CloseButton.Click -= CloseButton_Click;
        MaximizeButton.Click -= MaximizeButton_Click;
        MinimizeButton.Click -= MinimizeButton_Click;
        MoveDragArea.PointerPressed -= MoveDragArea_PointerPressed;

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
            PathGeometry.Parse("M19 13H5a1 1 0 0 1 0-2h14a1 1 0 0 1 0 2z"));

    public Geometry MinimizeIcon
    {
        get => GetValue(MinimizeIconProperty);
        set => SetValue(MinimizeIconProperty, value);
    }

    public static readonly StyledProperty<Geometry> MaximizeIconProperty =
        AvaloniaProperty.Register<OvoTitleBar, Geometry>(nameof(MaximizeIcon),
            PathGeometry.Parse("M18 21H6a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3zM6 5a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V6a1 1 0 0 0-1-1z"));

    public Geometry MaximizeIcon
    {
        get => GetValue(MaximizeIconProperty);
        set => SetValue(MaximizeIconProperty, value);
    }

    public static readonly StyledProperty<Geometry> RestoreIconProperty =
        AvaloniaProperty.Register<OvoTitleBar, Geometry>(nameof(RestoreIcon),
            PathGeometry.Parse(
                "M18 21H6a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3zM6 5a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V6a1 1 0 0 0-1-1z"));

    public Geometry RestoreIcon
    {
        get => GetValue(RestoreIconProperty);
        set => SetValue(RestoreIconProperty, value);
    }

    public static readonly StyledProperty<Geometry> CloseIconProperty =
        AvaloniaProperty.Register<OvoTitleBar, Geometry>(nameof(CloseIcon),
            PathGeometry.Parse("M13.41 12l4.3-4.29a1 1 0 1 0-1.42-1.42L12 10.59l-4.29-4.3a1 1 0 0 0-1.42 1.42l4.3 4.29-4.3 4.29a1 1 0 0 0 0 1.42 1 1 0 0 0 1.42 0l4.29-4.3 4.29 4.3a1 1 0 0 0 1.42 0 1 1 0 0 0 0-1.42z"));

    public Geometry CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

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
    //         Debug.WriteLine("TioTitleBar: IsPointerOver property not found");
    //         return;
    //     }
    //
    //     var window = TopLevel.GetTopLevel(this) as Window;
    //     if (window == null)
    //     {
    //         Debug.WriteLine("TioTitleBar: Window not found");
    //         return;
    //     }
    //
    //     Debug.WriteLine("TioTitleBar: Enabling Snap Layout for button");
    //
    //     try
    //     {
    //         _wndProcHookCallback = ProcHookCallback;
    //         Win32Properties.AddWndProcHookCallback(window, _wndProcHookCallback);
    //
    //         Debug.WriteLine("TioTitleBar: Win32 hook successfully registered");
    //     }
    //     catch (Exception ex)
    //     {
    //         Debug.WriteLine($"TioTitleBar: Failed to enable Windows Snap Layout: {ex.Message}");
    //         Debug.WriteLine($"TioTitleBar: Stack trace: {ex.StackTrace}");
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
    //                     // Debug.WriteLine("TioTitleBar: Pointer entered maximize button");
    //                 }
    //
    //                 var result = IsMouseDown() ? HTCLIENT : HTMAXBUTTON;
    //                 // Debug.WriteLine($"TioTitleBar: Returning {(result == HTMAXBUTTON ? "HTMAXBUTTON" : "HTCLIENT")}");
    //                 return result;
    //             }
    //
    //             if (!pointerOnButton) return 0;
    //             pointerOnButton = false;
    //             pointerOverSetter.SetValue(maximizeButton, false);
    //             // Debug.WriteLine("TioTitleBar: Pointer left maximize button");
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
