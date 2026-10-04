using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace OvoUi.Controls;

public class OvoWindow : Window
{
    private Action<OvoTitleBar>? _titleBarLoadedCallback;
    private OverlayDialogHost? _dialogHost;
    private CornerRadius _frameContentCornerRadius;

    static OvoWindow()
    {
        FrameBorderThicknessProperty.Changed.AddClassHandler<OvoWindow>((window, _) =>
            window.UpdateFrameContentCornerRadius());
        FrameBorderCornerRadiusProperty.Changed.AddClassHandler<OvoWindow>((window, _) =>
            window.UpdateFrameContentCornerRadius());
    }

    /*public TioWindow()
    {
        PropertyChanged += (_, args) =>
        {
            if (args.Property != WindowStateProperty) return;
            RootBorder?.Margin = new Thickness(WindowState == WindowState.Maximized ? 8 : 0);
        };
    }*/

    protected override Type StyleKeyOverride => typeof(OvoWindow);

    /// <summary>
    ///     获取 TitleBar 控件引用。此属性在模板应用后才可用。
    /// </summary>
    public OvoTitleBar? TitleBar { get; private set; }

    /// <summary>
    ///     获取根 Border 控件引用。此属性在模板应用后才可用。
    /// </summary>
    public Border? RootBorder { get; private set; }

    /// <summary>
    ///     当 TitleBar 加载完成时触发的事件
    /// </summary>
    public event EventHandler<OvoTitleBar>? TitleBarLoaded;

    /// <summary>
    ///     设置 TitleBar 加载完成后的回调。如果 TitleBar 已经加载，则立即执行回调。
    /// </summary>
    /// <param name="callback">回调函数，参数为 TitleBar 实例</param>
    public void OnTitleBarLoaded(Action<OvoTitleBar> callback)
    {
        if (TitleBar != null)
            callback(TitleBar);
        else
            _titleBarLoadedCallback = callback;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // 从模板中获取控件引用
        TitleBar = e.NameScope.Find<OvoTitleBar>("PART_TitleBar");
        RootBorder = e.NameScope.Find<Border>("PART_Root");
        _dialogHost = e.NameScope.Find<OverlayDialogHost>("PART_DialogHost");

        if (_dialogHost is not null)
        {
            LogicalChildren.Add(_dialogHost);
        }

        if (TitleBar != null)
        {
            TitleBarLoaded?.Invoke(this, TitleBar);
            _titleBarLoadedCallback?.Invoke(TitleBar);
            _titleBarLoadedCallback = null;

            // 设置 DialogHost 的 SafePadding
            UpdateDialogHostSafePadding();
        }

        // 初始化根 Border 的 Margin
        if (RootBorder != null) RootBorder.Margin = new Thickness(WindowState == WindowState.Maximized ? 10 : 0);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            PropertyChanged += (_, e) =>
            {
                if (e.Property.Name != nameof(WindowState)) return;
                RootBorder?.CornerRadius = new CornerRadius(WindowState == WindowState.Maximized ? 0 : 10);
                RootBorder?.BorderThickness = new Thickness(WindowState == WindowState.Maximized ? 0 : 1);
            };
        }
    }

    private void UpdateDialogHostSafePadding()
    {
        if (!IsEnabledDialogHostSafePadding) return;
        if (_dialogHost is null || TitleBar is null) return;

        var height = TitleBar.Bounds.Height;
        if (height == 0)
        {
            // 如果 TitleBar 还没有测量，使用默认高度
            height = TitleBarHeight;
        }

        var dialogHostPadding = new Thickness(0, height, 0, 0);
        _dialogHost.SafePadding = dialogHostPadding;
    }

    #region Styled Properties

    public static readonly StyledProperty<bool> IsCloseBtnShowProperty =
        AvaloniaProperty.Register<OvoWindow, bool>(nameof(IsCloseBtnShow), true);

    public bool IsCloseBtnShow
    {
        get => GetValue(IsCloseBtnShowProperty);
        set => SetValue(IsCloseBtnShowProperty, value);
    }

    public static readonly StyledProperty<bool> IsEnabledDialogHostSafePaddingProperty =
        AvaloniaProperty.Register<OvoWindow, bool>(nameof(IsEnabledDialogHostSafePadding), false);

    public bool IsEnabledDialogHostSafePadding
    {
        get => GetValue(IsEnabledDialogHostSafePaddingProperty);
        set => SetValue(IsEnabledDialogHostSafePaddingProperty, value);
    }

    public bool IsManagedResizerVisible
    {
        get => GetValue(IsManagedResizerVisibleProperty);
        set => SetValue(IsManagedResizerVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> IsManagedResizerVisibleProperty =
        AvaloniaProperty.Register<CustomDialogWindow, bool>(
            nameof(IsManagedResizerVisible));

    public static readonly StyledProperty<Thickness> TitleBarControlBtnMarginProperty =
        AvaloniaProperty.Register<OvoWindow, Thickness>(nameof(TitleBarControlBtnMargin), new Thickness(0, 0, 5, 0));

    public Thickness TitleBarControlBtnMargin
    {
        get => GetValue(TitleBarControlBtnMarginProperty);
        set => SetValue(TitleBarControlBtnMarginProperty, value);
    }

    public static readonly StyledProperty<Thickness> FrameBorderThicknessProperty =
        AvaloniaProperty.Register<OvoWindow, Thickness>(nameof(FrameBorderThickness));

    public Thickness FrameBorderThickness
    {
        get => GetValue(FrameBorderThicknessProperty);
        set => SetValue(FrameBorderThicknessProperty, value);
    }

    public static readonly StyledProperty<CornerRadius> FrameBorderCornerRadiusProperty =
        AvaloniaProperty.Register<OvoWindow, CornerRadius>(nameof(FrameBorderCornerRadius));

    public CornerRadius FrameBorderCornerRadius
    {
        get => GetValue(FrameBorderCornerRadiusProperty);
        set => SetValue(FrameBorderCornerRadiusProperty, value);
    }

    public static readonly DirectProperty<OvoWindow, CornerRadius> FrameContentCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<OvoWindow, CornerRadius>(
            nameof(FrameContentCornerRadius),
            window => window.FrameContentCornerRadius);

    public CornerRadius FrameContentCornerRadius
    {
        get => _frameContentCornerRadius;
        private set => SetAndRaise(FrameContentCornerRadiusProperty, ref _frameContentCornerRadius, value);
    }

    private void UpdateFrameContentCornerRadius()
    {
        var radius = FrameBorderCornerRadius;
        var thickness = FrameBorderThickness;
        FrameContentCornerRadius = new CornerRadius(
            Math.Max(0, radius.TopLeft - Math.Max(thickness.Top, thickness.Left)),
            Math.Max(0, radius.TopRight - Math.Max(thickness.Top, thickness.Right)),
            Math.Max(0, radius.BottomRight - Math.Max(thickness.Bottom, thickness.Right)),
            Math.Max(0, radius.BottomLeft - Math.Max(thickness.Bottom, thickness.Left)));
    }

    public static readonly StyledProperty<Brush> FrameBorderBrushProperty =
        AvaloniaProperty.Register<OvoWindow, Brush>(nameof(FrameBorderBrush));

    public Brush FrameBorderBrush
    {
        get => GetValue(FrameBorderBrushProperty);
        set => SetValue(FrameBorderBrushProperty, value);
    }

    public static readonly StyledProperty<double> TitleBarHeightProperty =
        AvaloniaProperty.Register<OvoWindow, double>(nameof(TitleBarHeight), 36);

    public double TitleBarHeight
    {
        get => GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }

    public static readonly StyledProperty<bool> IsMaxBtnShowProperty =
        AvaloniaProperty.Register<OvoWindow, bool>(nameof(IsMaxBtnShow), true);

    public bool IsMaxBtnShow
    {
        get => GetValue(IsMaxBtnShowProperty);
        set => SetValue(IsMaxBtnShowProperty, value);
    }

    public static readonly StyledProperty<bool> IsMinBtnShowProperty =
        AvaloniaProperty.Register<OvoWindow, bool>(nameof(IsMinBtnShow), true);

    public bool IsMinBtnShow
    {
        get => GetValue(IsMinBtnShowProperty);
        set => SetValue(IsMinBtnShowProperty, value);
    }

    public virtual bool OnClose()
    {
        return false;
    }

    public virtual bool OnMinimize()
    {
        return false;
    }

    public virtual bool OnMaximize()
    {
        return false;
    }

    public static readonly StyledProperty<object?> TitleBarLeftContentProperty =
        AvaloniaProperty.Register<OvoWindow, object?>(nameof(TitleBarLeftContent));

    public object? TitleBarLeftContent
    {
        get => GetValue(TitleBarLeftContentProperty);
        set => SetValue(TitleBarLeftContentProperty, value);
    }

    public static readonly StyledProperty<object?> TitleBarRightContentProperty =
        AvaloniaProperty.Register<OvoWindow, object?>(nameof(TitleBarRightContent));

    public object? TitleBarRightContent
    {
        get => GetValue(TitleBarRightContentProperty);
        set => SetValue(TitleBarRightContentProperty, value);
    }

    public static readonly StyledProperty<Thickness> ContentMarginProperty =
        AvaloniaProperty.Register<OvoWindow, Thickness>(nameof(ContentMargin), new Thickness(10));

    public Thickness ContentMargin
    {
        get => GetValue(ContentMarginProperty);
        set => SetValue(ContentMarginProperty, value);
    }

    public static readonly StyledProperty<Geometry> MinimizeIconProperty =
        AvaloniaProperty.Register<OvoWindow, Geometry>(nameof(MinimizeIcon),
            PathGeometry.Parse("M19 13H5a1 1 0 0 1 0-2h14a1 1 0 0 1 0 2z"));

    public Geometry MinimizeIcon
    {
        get => GetValue(MinimizeIconProperty);
        set => SetValue(MinimizeIconProperty, value);
    }

    public static readonly StyledProperty<Geometry> MaximizeIconProperty =
        AvaloniaProperty.Register<OvoWindow, Geometry>(nameof(MaximizeIcon),
            PathGeometry.Parse(
                "M18 21H6a3 3 0 0 1-3-3V6a3 3 0 0 1 3-3h12a3 3 0 0 1 3 3v12a3 3 0 0 1-3 3zM6 5a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V6a1 1 0 0 0-1-1z"));

    public Geometry MaximizeIcon
    {
        get => GetValue(MaximizeIconProperty);
        set => SetValue(MaximizeIconProperty, value);
    }

    public static readonly StyledProperty<Geometry> CloseIconProperty =
        AvaloniaProperty.Register<OvoWindow, Geometry>(nameof(CloseIcon),
            PathGeometry.Parse(
                "M13.41 12l4.3-4.29a1 1 0 1 0-1.42-1.42L12 10.59l-4.29-4.3a1 1 0 0 0-1.42 1.42l4.3 4.29-4.3 4.29a1 1 0 0 0 0 1.42 1 1 0 0 0 1.42 0l4.29-4.3 4.29 4.3a1 1 0 0 0 1.42 0 1 1 0 0 0 0-1.42z"));

    public Geometry CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    public string HostId { get; set; } = Guid.NewGuid().ToString();

    #endregion
}
