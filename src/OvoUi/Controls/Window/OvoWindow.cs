using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace OvoUi.Controls;

public class OvoWindow : Window
{
    public enum TitleBarVisibilityMode
    {
        Unchanged,
        Visible,
        Hidden,
        AutoHidden
    }

    private static readonly TimeSpan DefaultFullScreenTransitionDuration = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan DefaultTitleBarAnimationDuration = TimeSpan.FromMilliseconds(350);

    private readonly DispatcherTimer _hideTitleBarTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(1000)
    };

    private readonly DispatcherTimer _showTitleBarTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(300)
    };

    private Action<OvoTitleBar>? _titleBarLoadedCallback;
    private OverlayDialogHost? _dialogHost;
    private LayoutTransformControl? _titleBarAnimationHost;
    private CancellationTokenSource? _titleBarAnimationCancellation;
    private CornerRadius _frameContentCornerRadius;
    private WindowState _previousVisibleWindowState = WindowState.Normal;
    private WindowState _windowStateBeforeFullScreen = WindowState.Normal;
    private bool _wasTitleBarVisibleBeforeFullScreen = true;
    private bool _hasCapturedPreFullScreenState;
    private bool _isFullScreenTransitioning;

    static OvoWindow()
    {
        FrameBorderThicknessProperty.Changed.AddClassHandler<OvoWindow>((window, _) =>
            window.UpdateFrameContentCornerRadius());
        FrameBorderCornerRadiusProperty.Changed.AddClassHandler<OvoWindow>((window, _) =>
            window.UpdateFrameContentCornerRadius());
    }

    public OvoWindow()
    {
        _hideTitleBarTimer.Tick += HideTitleBarTimerOnTick;
        _showTitleBarTimer.Tick += ShowTitleBarTimerOnTick;
    }

    protected override Type StyleKeyOverride => typeof(OvoWindow);

    public OvoTitleBar? TitleBar { get; private set; }

    public Border? RootBorder { get; private set; }
    
    public static readonly DirectProperty<OvoWindow, WindowState> PreviousVisibleWindowStateProperty =
        AvaloniaProperty.RegisterDirect<OvoWindow, WindowState>(
            nameof(PreviousVisibleWindowState),
            window => window.PreviousVisibleWindowState);

    public WindowState PreviousVisibleWindowState
    {
        get => _previousVisibleWindowState;
        private set => SetAndRaise(PreviousVisibleWindowStateProperty, ref _previousVisibleWindowState, value);
    }

    public event EventHandler<OvoTitleBar>? TitleBarLoaded;
    
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

        CancelTitleBarAnimation();
        if (_dialogHost is not null) LogicalChildren.Remove(_dialogHost);
        if (TitleBar is not null) TitleBar.SizeChanged -= TitleBar_SizeChanged;

        TitleBar = e.NameScope.Find<OvoTitleBar>("PART_TitleBar");
        RootBorder = e.NameScope.Find<Border>("PART_Root");
        _dialogHost = e.NameScope.Find<OverlayDialogHost>("PART_DialogHost");
        _titleBarAnimationHost = e.NameScope.Find<LayoutTransformControl>("PART_TitleBarAnimationHost");

        if (_dialogHost is not null) LogicalChildren.Add(_dialogHost);

        if (TitleBar != null)
        {
            TitleBar.SizeChanged += TitleBar_SizeChanged;
            TitleBar.SetFullScreenMode(WindowState == WindowState.FullScreen);
            TitleBarLoaded?.Invoke(this, TitleBar);
            _titleBarLoadedCallback?.Invoke(TitleBar);
            _titleBarLoadedCallback = null;
        }

        if (_titleBarAnimationHost is not null)
            _titleBarAnimationHost.IsVisible = IsTitleBarVisible;

        UpdateWindowFrame();
        UpdateDialogHostSafePadding();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WindowStateProperty &&
            change.OldValue is WindowState oldState &&
            change.NewValue is WindowState newState)
        {
            OnWindowStateChanged(oldState, newState);
            UpdateWindowFrame();
        }
        else if (change.Property == IsTitleBarVisibleProperty)
        {
            AnimateTitleBar(change.GetNewValue<bool>());
            UpdateDialogHostSafePadding();
        }
        else if (change.Property == TitleBarVisibilityOnFullScreenProperty)
        {
            UpdateFullScreenTitleBarBehavior();
        }
        else if (change.Property == TitleBarAutoHideDelayProperty)
        {
            _hideTitleBarTimer.Interval = TimeSpan.FromMilliseconds(TitleBarAutoHideDelay);
        }
        else if (change.Property == TitleBarAutoShowDelayProperty)
        {
            _showTitleBarTimer.Interval = TimeSpan.FromMilliseconds(TitleBarAutoShowDelay);
        }
        else if (change.Property == OvoTitleBar.TitleBarHeightProperty ||
                 change.Property == IsDialogHostSafePaddingEnabledProperty)
        {
            UpdateDialogHostSafePadding();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        StopTitleBarAutoHide();
        CancelTitleBarAnimation();
        _hideTitleBarTimer.Tick -= HideTitleBarTimerOnTick;
        _showTitleBarTimer.Tick -= ShowTitleBarTimerOnTick;
        base.OnClosed(e);
    }

    private void TitleBar_SizeChanged(object? sender, SizeChangedEventArgs e) => UpdateDialogHostSafePadding();

    private void OnWindowStateChanged(WindowState oldState, WindowState newState)
    {
        StopTitleBarAutoHide();

        if (!_isFullScreenTransitioning && newState is WindowState.Normal or WindowState.Maximized)
            PreviousVisibleWindowState = newState;

        if (newState == WindowState.FullScreen)
        {
            if (!_hasCapturedPreFullScreenState)
            {
                var previousState = oldState == WindowState.Minimized ? PreviousVisibleWindowState : oldState;
                _windowStateBeforeFullScreen = previousState == WindowState.Maximized
                    ? WindowState.Maximized
                    : WindowState.Normal;
                _wasTitleBarVisibleBeforeFullScreen = IsTitleBarVisible;
                _hasCapturedPreFullScreenState = true;
            }

            ApplyFullScreenTitleBarBehavior();
        }
        else if (oldState == WindowState.FullScreen)
        {
            if (TitleBarVisibilityOnFullScreen != TitleBarVisibilityMode.Unchanged)
                SetCurrentValue(IsTitleBarVisibleProperty, _wasTitleBarVisibleBeforeFullScreen);

            _hasCapturedPreFullScreenState = false;
        }

        TitleBar?.SetFullScreenMode(newState == WindowState.FullScreen);
    }

    private void UpdateFullScreenTitleBarBehavior()
    {
        if (WindowState != WindowState.FullScreen) return;

        StopTitleBarAutoHide();
        SetCurrentValue(IsTitleBarVisibleProperty, TitleBarVisibilityOnFullScreen switch
        {
            TitleBarVisibilityMode.Unchanged => _wasTitleBarVisibleBeforeFullScreen,
            TitleBarVisibilityMode.Visible => true,
            TitleBarVisibilityMode.Hidden or TitleBarVisibilityMode.AutoHidden => false,
            _ => IsTitleBarVisible
        });

        if (TitleBarVisibilityOnFullScreen == TitleBarVisibilityMode.AutoHidden)
            PointerMoved += AutoHideTitleBarOnPointerMoved;
    }

    private void ApplyFullScreenTitleBarBehavior()
    {
        switch (TitleBarVisibilityOnFullScreen)
        {
            case TitleBarVisibilityMode.Visible:
                SetCurrentValue(IsTitleBarVisibleProperty, true);
                break;
            case TitleBarVisibilityMode.Hidden:
                SetCurrentValue(IsTitleBarVisibleProperty, false);
                break;
            case TitleBarVisibilityMode.AutoHidden:
                if (IsTitleBarVisible) _hideTitleBarTimer.Start();
                PointerMoved += AutoHideTitleBarOnPointerMoved;
                break;
        }
    }

    private void AutoHideTitleBarOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (WindowState != WindowState.FullScreen ||
            TitleBarVisibilityOnFullScreen != TitleBarVisibilityMode.AutoHidden)
            return;

        var position = e.GetPosition(this);
        if (position.Y <= 10)
        {
            _hideTitleBarTimer.Stop();
            if (!IsTitleBarVisible) _showTitleBarTimer.Start();
        }
        else if (position.Y >= 50)
        {
            _showTitleBarTimer.Stop();
            if (IsTitleBarVisible) _hideTitleBarTimer.Start();
        }
    }

    private void HideTitleBarTimerOnTick(object? sender, EventArgs e)
    {
        _hideTitleBarTimer.Stop();
        if (WindowState == WindowState.FullScreen &&
            TitleBarVisibilityOnFullScreen == TitleBarVisibilityMode.AutoHidden)
            SetCurrentValue(IsTitleBarVisibleProperty, false);
    }

    private void ShowTitleBarTimerOnTick(object? sender, EventArgs e)
    {
        _showTitleBarTimer.Stop();
        if (WindowState == WindowState.FullScreen &&
            TitleBarVisibilityOnFullScreen == TitleBarVisibilityMode.AutoHidden)
            SetCurrentValue(IsTitleBarVisibleProperty, true);
    }

    private void StopTitleBarAutoHide()
    {
        PointerMoved -= AutoHideTitleBarOnPointerMoved;
        _hideTitleBarTimer.Stop();
        _showTitleBarTimer.Stop();
    }

    private async void AnimateTitleBar(bool isVisible)
    {
        var host = _titleBarAnimationHost;
        if (host is null) return;

        CancelTitleBarAnimation();
        if (!TitleBarAnimationEnabled || !IsLoaded)
        {
            host.IsVisible = isVisible;
            return;
        }

        var cancellation = new CancellationTokenSource();
        _titleBarAnimationCancellation = cancellation;
        if (isVisible) host.IsVisible = true;

        var animation = new Animation
        {
            Duration = DefaultTitleBarAnimationDuration,
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame
                {
                    KeyTime = TimeSpan.Zero,
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleYProperty, isVisible ? 0d : 1d)
                    }
                },
                new KeyFrame
                {
                    KeyTime = DefaultTitleBarAnimationDuration,
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleYProperty, isVisible ? 1d : 0d)
                    }
                }
            }
        };

        try
        {
            await animation.RunAsync(host, cancellation.Token);
            if (!cancellation.IsCancellationRequested && !isVisible)
                host.IsVisible = false;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void CancelTitleBarAnimation()
    {
        _titleBarAnimationCancellation?.Cancel();
        _titleBarAnimationCancellation?.Dispose();
        _titleBarAnimationCancellation = null;
    }

    private void UpdateWindowFrame()
    {
        if (RootBorder is null || !RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;
        var hasSquareFrame = WindowState is WindowState.Maximized or WindowState.FullScreen;
        RootBorder.CornerRadius = new CornerRadius(hasSquareFrame ? 0 : 10);
        RootBorder.BorderThickness = new Thickness(hasSquareFrame ? 0 : 1);
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
        if (height == 0) height = OvoTitleBar.GetTitleBarHeight(this);
        _dialogHost.SafePadding = new Thickness(0, height, 0, 0);
    }

    public void ToggleFullScreen() => _ = ToggleFullScreenAsync();
    
    public async Task ToggleFullScreenAsync()
    {
        if (_isFullScreenTransitioning) return;

        _isFullScreenTransitioning = true;
        try
        {
            if (WindowState == WindowState.FullScreen)
            {
                var restoreState = _windowStateBeforeFullScreen;
                if (ShouldAnimateFullScreenTransition())
                {
                    WindowState = WindowState.Maximized;
                    await Task.Delay(FullScreenTransitionDuration);
                }

                WindowState = restoreState;
                return;
            }

            _windowStateBeforeFullScreen = WindowState == WindowState.Maximized
                ? WindowState.Maximized
                : WindowState.Normal;
            _wasTitleBarVisibleBeforeFullScreen = IsTitleBarVisible;
            _hasCapturedPreFullScreenState = true;

            if (ShouldAnimateFullScreenTransition() && WindowState != WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
                await Task.Delay(FullScreenTransitionDuration);
            }

            WindowState = WindowState.FullScreen;
        }
        finally
        {
            _isFullScreenTransitioning = false;
            if (WindowState is WindowState.Normal or WindowState.Maximized)
                PreviousVisibleWindowState = WindowState;
        }
    }

    private bool ShouldAnimateFullScreenTransition() =>
        IsFullScreenTransitionAnimationEnabled &&
        FullScreenTransitionDuration > TimeSpan.Zero &&
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    #region Styled Properties

    public static readonly StyledProperty<bool> IsTitleBarVisibleProperty =
        AvaloniaProperty.Register<OvoWindow, bool>(nameof(IsTitleBarVisible), true);

    public bool IsTitleBarVisible
    {
        get => GetValue(IsTitleBarVisibleProperty);
        set => SetValue(IsTitleBarVisibleProperty, value);
    }

    public static readonly StyledProperty<TitleBarVisibilityMode> TitleBarVisibilityOnFullScreenProperty =
        AvaloniaProperty.Register<OvoWindow, TitleBarVisibilityMode>(nameof(TitleBarVisibilityOnFullScreen),
            TitleBarVisibilityMode.AutoHidden);

    public TitleBarVisibilityMode TitleBarVisibilityOnFullScreen
    {
        get => GetValue(TitleBarVisibilityOnFullScreenProperty);
        set => SetValue(TitleBarVisibilityOnFullScreenProperty, value);
    }

    public static readonly StyledProperty<int> TitleBarAutoHideDelayProperty =
        AvaloniaProperty.Register<OvoWindow, int>(nameof(TitleBarAutoHideDelay), 1000,
            coerce: (_, value) => Math.Max(0, value));

    public int TitleBarAutoHideDelay
    {
        get => GetValue(TitleBarAutoHideDelayProperty);
        set => SetValue(TitleBarAutoHideDelayProperty, value);
    }

    public static readonly StyledProperty<int> TitleBarAutoShowDelayProperty =
        AvaloniaProperty.Register<OvoWindow, int>(nameof(TitleBarAutoShowDelay), 300,
            coerce: (_, value) => Math.Max(0, value));

    public int TitleBarAutoShowDelay
    {
        get => GetValue(TitleBarAutoShowDelayProperty);
        set => SetValue(TitleBarAutoShowDelayProperty, value);
    }

    public static readonly StyledProperty<bool> TitleBarAnimationEnabledProperty =
        AvaloniaProperty.Register<OvoWindow, bool>(nameof(TitleBarAnimationEnabled), true);

    public bool TitleBarAnimationEnabled
    {
        get => GetValue(TitleBarAnimationEnabledProperty);
        set => SetValue(TitleBarAnimationEnabledProperty, value);
    }

    public static readonly StyledProperty<bool> IsFullScreenTransitionAnimationEnabledProperty =
        AvaloniaProperty.Register<OvoWindow, bool>(nameof(IsFullScreenTransitionAnimationEnabled), true);

    public bool IsFullScreenTransitionAnimationEnabled
    {
        get => GetValue(IsFullScreenTransitionAnimationEnabledProperty);
        set => SetValue(IsFullScreenTransitionAnimationEnabledProperty, value);
    }

    public static readonly StyledProperty<TimeSpan> FullScreenTransitionDurationProperty =
        AvaloniaProperty.Register<OvoWindow, TimeSpan>(nameof(FullScreenTransitionDuration),
            DefaultFullScreenTransitionDuration,
            coerce: (_, value) => value < TimeSpan.Zero ? TimeSpan.Zero : value);

    public TimeSpan FullScreenTransitionDuration
    {
        get => GetValue(FullScreenTransitionDurationProperty);
        set => SetValue(FullScreenTransitionDurationProperty, value);
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
        AvaloniaProperty.Register<OvoWindow, bool>(nameof(IsManagedResizerVisible));

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

    public virtual bool OnClose() => false;

    public virtual bool OnMinimize() => false;

    public virtual bool OnMaximize() => false;

    public virtual bool OnFullScreen() => false;

    public virtual bool OnPin() => false;

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
