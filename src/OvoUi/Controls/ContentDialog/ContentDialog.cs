using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace OvoUi.Controls;

public partial class ContentDialog : ContentControl
{
    private Button? _primaryButton;
    private Button? _secondaryButton;
    private Button? _closeButton;
    private ContentDialogHost? _dialogHost;
    private OverlayDialogHost? _overlayHost;
    private IDisposable? _hostBoundsSubscription;
    private IInputElement? _lastFocus;
    private Control? _originalParent;
    private int _originalIndex;
    private TaskCompletionSource<ContentDialogResult>? _completion;
    private ContentDialogResult _result;
    private bool _deferralActive;
    private bool _isClosing;
    private CancellationTokenSource? _openingAnimation;

    public ContentDialog()
    {
        PseudoClasses.Add(PC_Hidden);
        KeyboardNavigation.SetTabNavigation(this, KeyboardNavigationMode.Cycle);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_primaryButton is not null) _primaryButton.Click -= OnButtonClick;
        if (_secondaryButton is not null) _secondaryButton.Click -= OnButtonClick;
        if (_closeButton is not null) _closeButton.Click -= OnButtonClick;
        base.OnApplyTemplate(e);
        _primaryButton = e.NameScope.Get<Button>(PART_PrimaryButton);
        _secondaryButton = e.NameScope.Get<Button>(PART_SecondaryButton);
        _closeButton = e.NameScope.Get<Button>(PART_CloseButton);
        _primaryButton.Click += OnButtonClick;
        _secondaryButton.Click += OnButtonClick;
        _closeButton.Click += OnButtonClick;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FullSizeDesiredProperty)
            PseudoClasses.Set(PC_FullSize, change.GetNewValue<bool>());
    }

    protected override bool RegisterContentPresenter(ContentPresenter presenter) =>
        presenter.Name == "PART_ContentPresenter" || base.RegisterContentPresenter(presenter);

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (!e.Handled && e.Key == Key.Escape)
        {
            Hide();
            e.Handled = true;
        }
        else if (!e.Handled && e.Key == Key.Enter)
        {
            var button = DefaultButton switch
            {
                ContentDialogButton.Primary => _primaryButton,
                ContentDialogButton.Secondary => _secondaryButton,
                ContentDialogButton.Close => _closeButton,
                _ => null
            };
            if (button?.IsEffectivelyEnabled == true && button.IsEffectivelyVisible)
            {
                OnButtonClick(button, null);
                e.Handled = true;
            }
        }

        base.OnKeyUp(e);
    }

    public Task<ContentDialogResult> ShowAsync() => ShowAsyncCore(null, null);
    public Task<ContentDialogResult> ShowAsync(Window window) => ShowAsyncCore(window, null);
    public Task<ContentDialogResult> ShowAsync(TopLevel topLevel) => ShowAsyncCore(topLevel, null);
    public Task<ContentDialogResult> ShowAsync(string? hostId) => ShowAsyncCore(null, hostId);

    public void Hide() => Hide(ContentDialogResult.None);

    public void Hide(ContentDialogResult result)
    {
        if (_completion is null || _isClosing) return;
        _result = result;
        BeginClose();
    }

    protected virtual void OnOpening() => Opening?.Invoke(this, EventArgs.Empty);
    protected virtual void OnOpened() => Opened?.Invoke(this, EventArgs.Empty);
    protected virtual void OnClosing(ContentDialogClosingEventArgs args) => Closing?.Invoke(this, args);
    protected virtual void OnClosed(ContentDialogClosedEventArgs args) => Closed?.Invoke(this, args);

    protected virtual void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args) =>
        PrimaryButtonClick?.Invoke(this, args);

    protected virtual void OnSecondaryButtonClick(ContentDialogButtonClickEventArgs args) =>
        SecondaryButtonClick?.Invoke(this, args);

    protected virtual void OnCloseButtonClick(ContentDialogButtonClickEventArgs args) =>
        CloseButtonClick?.Invoke(this, args);

    private Task<ContentDialogResult> ShowAsyncCore(TopLevel? topLevel, string? hostId)
    {
        if (_completion is not null)
            throw new InvalidOperationException("This ContentDialog is already open.");

        var overlayHost = FindOverlayDialogHost(topLevel, hostId) ??
                          throw new InvalidOperationException(
                              "OverlayDialogHost was not found. Use OvoWindow/OvoView or add an OverlayDialogHost to the view.");

        _completion = new TaskCompletionSource<ContentDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        _result = ContentDialogResult.None;
        _isClosing = false;
        OnOpening();
        DetachFromOriginalParent();

        _overlayHost = overlayHost;
        _lastFocus = TopLevel.GetTopLevel(overlayHost)?.FocusManager?.GetFocusedElement();
        PseudoClasses.Set(PC_Primary, !string.IsNullOrWhiteSpace(PrimaryButtonText));
        PseudoClasses.Set(PC_Secondary, !string.IsNullOrWhiteSpace(SecondaryButtonText));
        PseudoClasses.Set(PC_Close, !string.IsNullOrWhiteSpace(CloseButtonText));

        _dialogHost ??= new ContentDialogHost();
        // Loaded runs after attachment. Keep the entire layer hidden until both
        // the mask and dialog have their opening values, including on reuse.
        _dialogHost.SetCurrentValue(OpacityProperty, 0.0);
        _dialogHost.Content = this;
        SyncHostSize(overlayHost.Bounds.Size);
        Canvas.SetLeft(_dialogHost, 0);
        Canvas.SetTop(_dialogHost, 0);
        _hostBoundsSubscription =
            overlayHost.GetObservable(BoundsProperty).Subscribe(bounds => SyncHostSize(bounds.Size));
        // TransformAnimator requires a ScaleTransform, rather than CSS TransformOperations.
        RenderTransform = new ScaleTransform(0.92, 0.92);
        IsVisible = true;
        IsHitTestVisible = true;
        PseudoClasses.Set(PC_Hidden, false);
        PseudoClasses.Set(PC_Open, true);
        SetCurrentValue(OpacityProperty, 0.0);
        Loaded += OnDialogLoaded;
        overlayHost.Children.Add(_dialogHost);
        return _completion.Task;
    }

    private async void OnDialogLoaded(object? sender, RoutedEventArgs e)
    {
        if (e.Source != this) return;
        Loaded -= OnDialogLoaded;
        if (_completion is null || _isClosing) return;
        var animate = _overlayHost?.IsAnimationDisabled == false;
        SetCurrentValue(OpacityProperty, animate ? 0.0 : 1.0);
        _dialogHost?.OverlayMask?.SetCurrentValue(OpacityProperty, animate ? 0.0 : 1.0);
        if (!animate) RenderTransform = new ScaleTransform();
        var openingAnimation = new CancellationTokenSource();
        _openingAnimation = openingAnimation;
        var animation = animate
            ? Task.WhenAll(
                OverlayDialogHost.DialogAppearAnimation.RunAsync(this, openingAnimation.Token),
                _dialogHost?.OverlayMask is { } mask
                    ? OverlayDialogHost.MaskAppearAnimation.RunAsync(mask, openingAnimation.Token)
                    : Task.CompletedTask)
            : Task.CompletedTask;
        _dialogHost?.SetCurrentValue(OpacityProperty, 1.0);
        _primaryButton?.Classes.Remove("accent");
        _secondaryButton?.Classes.Remove("accent");
        _closeButton?.Classes.Remove("accent");
        var defaultButton = DefaultButton switch
        {
            ContentDialogButton.Primary when _primaryButton?.IsVisible == true => _primaryButton,
            ContentDialogButton.Secondary when _secondaryButton?.IsVisible == true => _secondaryButton,
            ContentDialogButton.Close when _closeButton?.IsVisible == true => _closeButton,
            _ => null
        };
        defaultButton?.Classes.Add("accent");
        (defaultButton as IInputElement ?? this).Focus();
        OnOpened();
        try
        {
            await animation;
        }
        catch (OperationCanceledException) when (openingAnimation.IsCancellationRequested)
        {
            // Closing supersedes the opening animation.
        }
        finally
        {
            if (_openingAnimation == openingAnimation) _openingAnimation = null;
            openingAnimation.Dispose();
        }
    }

    private void OnButtonClick(object? sender, RoutedEventArgs? e)
    {
        if (_deferralActive || _completion is null || _isClosing) return;
        _deferralActive = true;
        var args = new ContentDialogButtonClickEventArgs();
        args.Begin(() =>
        {
            _deferralActive = false;
            if (args.Cancel) return;
            if (sender == _primaryButton)
            {
                Execute(PrimaryButtonCommand, PrimaryButtonCommandParameter);
                _result = ContentDialogResult.Primary;
            }
            else if (sender == _secondaryButton)
            {
                Execute(SecondaryButtonCommand, SecondaryButtonCommandParameter);
                _result = ContentDialogResult.Secondary;
            }
            else
            {
                Execute(CloseButtonCommand, CloseButtonCommandParameter);
                _result = ContentDialogResult.None;
            }

            BeginClose();
        });

        if (sender == _primaryButton) OnPrimaryButtonClick(args);
        else if (sender == _secondaryButton) OnSecondaryButtonClick(args);
        else OnCloseButtonClick(args);
        args.CompleteDeferral();
    }

    private void BeginClose()
    {
        if (_deferralActive || _completion is null || _isClosing) return;
        _deferralActive = true;
        var args = new ContentDialogClosingEventArgs(_result);
        args.Begin(() =>
        {
            _deferralActive = false;
            if (!args.Cancel) FinalCloseDialog();
        });
        OnClosing(args);
        args.CompleteDeferral();
    }

    private async void FinalCloseDialog()
    {
        var completion = _completion;
        if (completion is null || _isClosing) return;
        _isClosing = true;
        Loaded -= OnDialogLoaded;
        _openingAnimation?.Cancel();
        IsHitTestVisible = false;
        PseudoClasses.Set(PC_Hidden, true);
        PseudoClasses.Set(PC_Open, false);
        if (_overlayHost?.IsAnimationDisabled == false)
        {
            await Task.WhenAll(
                OverlayDialogHost.DialogDisappearAnimation.RunAsync(this),
                _dialogHost?.OverlayMask is { } mask
                    ? OverlayDialogHost.MaskDisappearAnimation.RunAsync(mask)
                    : Task.CompletedTask);
        }

        _hostBoundsSubscription?.Dispose();
        _hostBoundsSubscription = null;
        _overlayHost?.Children.Remove(_dialogHost!);
        _overlayHost = null;
        _dialogHost!.Content = null;
        RestoreOriginalParent();
        IsVisible = false;
        IsHitTestVisible = true;
        _lastFocus?.Focus();
        _lastFocus = null;
        _completion = null;
        OnClosed(new ContentDialogClosedEventArgs(_result));
        completion.TrySetResult(_result);
    }

    private void SyncHostSize(Size size)
    {
        if (_dialogHost is null) return;
        _dialogHost.Width = size.Width;
        _dialogHost.Height = size.Height;
    }

    private void DetachFromOriginalParent()
    {
        _originalParent = Parent as Control;
        switch (_originalParent)
        {
            case Panel panel:
                _originalIndex = panel.Children.IndexOf(this);
                panel.Children.Remove(this);
                break;
            case Decorator decorator:
                decorator.Child = null;
                break;
            case ContentControl contentControl:
                contentControl.Content = null;
                break;
            case ContentPresenter presenter:
                presenter.Content = null;
                break;
        }
    }

    private void RestoreOriginalParent()
    {
        switch (_originalParent)
        {
            case Panel panel:
                panel.Children.Insert(Math.Min(_originalIndex, panel.Children.Count), this);
                break;
            case Decorator decorator:
                decorator.Child = this;
                break;
            case ContentControl contentControl:
                contentControl.Content = this;
                break;
            case ContentPresenter presenter:
                presenter.Content = this;
                break;
        }

        _originalParent = null;
    }

    private static void Execute(System.Windows.Input.ICommand? command, object? parameter)
    {
        if (command?.CanExecute(parameter) == true) command.Execute(parameter);
    }

    private static OverlayDialogHost? FindOverlayDialogHost(TopLevel? topLevel, string? hostId)
    {
        topLevel ??= FindActiveTopLevel();
        if (topLevel is not null)
            return OverlayDialogManager.GetHost(hostId, topLevel.GetHashCode()) ??
                   topLevel.GetVisualDescendants().OfType<OverlayDialogHost>().FirstOrDefault();
        return OverlayDialogManager.GetHost(hostId, null);
    }

    private static TopLevel? FindActiveTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.Windows.FirstOrDefault(window => window.IsActive) ?? desktop.MainWindow;
        if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime single)
            return TopLevel.GetTopLevel(single.MainView);
        return null;
    }
}
