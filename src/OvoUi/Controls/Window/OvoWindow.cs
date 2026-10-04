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

    public OvoWindow()
    {
        PropertyChanged += (_, change) =>
        {
            if (change.Property == WindowStateProperty)
                UpdateWindowFrame();
            if (change.Property == IsTitleBarVisibleProperty ||
                change.Property == OvoTitleBar.TitleBarHeightProperty ||
                change.Property == IsDialogHostSafePaddingEnabledProperty)
                UpdateDialogHostSafePadding();
        };
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

        if (_dialogHost is not null) LogicalChildren.Remove(_dialogHost);
        if (TitleBar is not null) TitleBar.SizeChanged -= TitleBar_SizeChanged;

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
            TitleBar.SizeChanged += TitleBar_SizeChanged;
            TitleBarLoaded?.Invoke(this, TitleBar);
            _titleBarLoadedCallback?.Invoke(TitleBar);
            _titleBarLoadedCallback = null;

            // 设置 DialogHost 的 SafePadding
            UpdateDialogHostSafePadding();
        }

        UpdateWindowFrame();
        UpdateDialogHostSafePadding();
    }

    private void TitleBar_SizeChanged(object? sender, SizeChangedEventArgs e) => UpdateDialogHostSafePadding();

    private void UpdateWindowFrame()
    {
        if (RootBorder is null) return;
        // RootBorder.Margin = new Thickness(WindowState == WindowState.Maximized ? 10 : 0);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;
        RootBorder.CornerRadius = new CornerRadius(WindowState == WindowState.Maximized ? 0 : 10);
        RootBorder.BorderThickness = new Thickness(WindowState == WindowState.Maximized ? 0 : 1);
    }

    private void UpdateDialogHostSafePadding()
    {
        if (_dialogHost is null) return;
        if (!IsDialogHostSafePaddingEnabled || !IsTitleBarVisible || TitleBar is null)
        {
            _dialogHost.SafePadding = default;
            return;
        }

        var height = TitleBar.Bounds.Height;
        if (height == 0)
        {
            // 如果 TitleBar 还没有测量，使用默认高度
            height = OvoTitleBar.GetTitleBarHeight(this);
        }

        var dialogHostPadding = new Thickness(0, height, 0, 0);
        _dialogHost.SafePadding = dialogHostPadding;
    }

    #region Styled Properties

    public static readonly StyledProperty<bool> IsTitleBarVisibleProperty =
        AvaloniaProperty.Register<OvoWindow, bool>(nameof(IsTitleBarVisible), true);

    /// <summary>是否显示模板自带的标题栏；隐藏后可以在 Content 中自行放置 OvoTitleBar。</summary>
    public bool IsTitleBarVisible
    {
        get => GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> IsDialogHostSafePaddingEnabledProperty =
        AvaloniaProperty.Register<OvoWindow, bool>(nameof(IsDialogHostSafePaddingEnabled), false);

    public bool IsDialogHostSafePaddingEnabled
    {
        get => GetValue(IsDialogHostSafePaddingEnabledProperty);
        set => SetValue(IsDialogHostSafePaddingEnabledProperty, value);
    }

    public bool IsManagedResizerVisible
    {
        get => GetValue(IsManagedResizerVisibleProperty);
        set => SetValue(IsManagedResizerVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> IsManagedResizerVisibleProperty =
        AvaloniaProperty.Register<OvoWindow, bool>(
            nameof(IsManagedResizerVisible));

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

    public virtual bool OnFullScreen()
    {
        return false;
    }

    public virtual bool OnPin()
    {
        return false;
    }

    public static readonly StyledProperty<Thickness> ContentMarginProperty =
        AvaloniaProperty.Register<OvoWindow, Thickness>(nameof(ContentMargin), new Thickness(10));

    public Thickness ContentMargin
    {
        get => GetValue(ContentMarginProperty);
        set => SetValue(ContentMarginProperty, value);
    }

    public string HostId { get; set; } = Guid.NewGuid().ToString();

    #endregion
}
