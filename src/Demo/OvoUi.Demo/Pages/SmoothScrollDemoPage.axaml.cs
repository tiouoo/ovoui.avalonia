using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using OvoUi.Demo.Models;
using OvoUi.SmoothScroll.Controls;
using OvoUi.SmoothScroll.Controls.Easings;

namespace OvoUi.Demo.Pages;

public partial class SmoothScrollDemoPage : UserControl
{
    private OvoScrollPresenter? _listPresenter;
    private CancellationTokenSource? _resizeCancellation;
    private int _scrollChanged;
    private SmoothScrollDemoModel Model => (SmoothScrollDemoModel)DataContext!;

    public SmoothScrollDemoPage()
    {
        InitializeComponent();
        DataContext = new SmoothScrollDemoModel();
    }

    private void Zoom_OnClick(object? sender, RoutedEventArgs e) =>
        ReferenceZoom.ZoomTo(ReferenceZoom.ZoomFactor * ((sender as Button)?.Tag as string == "in" ? 1.2 : 1 / 1.2));

    private void BringItem_OnClick(object? sender, RoutedEventArgs e)
    {
        ReferenceList.SelectedIndex = Random.Shared.Next(ReferenceList.ItemCount);
        ReferenceList.ScrollIntoView(ReferenceList.SelectedIndex);
    }

    private void List_OnLoaded(object? sender, RoutedEventArgs e)
    {
        List_OnUnloaded(sender, e);
        _listPresenter = ReferenceList.GetVisualDescendants().OfType<OvoScrollPresenter>().FirstOrDefault();
        if (_listPresenter is not null)
            _listPresenter.ScrollAnimationStarting += List_OnAnimation;
    }

    private void List_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (_listPresenter is not null)
            _listPresenter.ScrollAnimationStarting -= List_OnAnimation;
        _listPresenter = null;
    }

    private void List_OnAnimation(object? sender, ScrollAnimationStartingEventArgs e)
    {
        if (AnimationMode.SelectedIndex != 1) return;
        var animation = e.Animation.Compositor.CreateVector3DKeyFrameAnimation();
        var delta = e.EndPosition.Y - e.StartingPosition.Y;
        animation.InsertKeyFrame(0.499999f, new Vector3D(e.StartingPosition.X, e.EndPosition.Y - 0.9 * delta, 0), new SplineEasing(1, 0, 1, 0));
        animation.InsertKeyFrame(0.5f, new Vector3D(e.StartingPosition.X, e.EndPosition.Y - 0.1 * delta, 0), new StepEasing());
        animation.InsertKeyFrame(1, new Vector3D(e.EndPosition.X, e.EndPosition.Y, 0), new SplineEasing(0, 1, 0, 1));
        animation.Duration = TimeSpan.FromMilliseconds(1500);
        e.Animation = animation;
    }

    private void Anchor_Loaded(object? sender, RoutedEventArgs e)
    {
        // ContainerPrepared can run before the item joins the presenter's visual tree.
        if (sender is Control element && ReferenceAnchors.IsVisualAncestorOf(element))
            ReferenceAnchors.RegisterAnchorCandidate(element);
    }

    private void Anchor_Unloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is Control element)
            ReferenceAnchors.UnregisterAnchorCandidate(element);
    }
    private void Anchors_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        _scrollChanged++;
        ReferenceAnchorStatus.Text = $"Anchor: {(ReferenceAnchors.CurrentAnchor?.DataContext as SmoothAnchorItem)?.Title ?? "none"} · ScrollChanged: {_scrollChanged}";
    }

    private async void Resize_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_resizeCancellation is not null) return;
        using var cancellation = new CancellationTokenSource();
        _resizeCancellation = cancellation;
        ReferenceResize.IsEnabled = false;
        try
        {
            foreach (var row in Model.AnchorItems)
            {
                cancellation.Token.ThrowIfCancellationRequested();
                row.Height = row.Height == 40 ? 80 : 40;
                await Task.Delay(5, cancellation.Token);
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        finally
        {
            _resizeCancellation = null;
            ReferenceResize.IsEnabled = true;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _resizeCancellation?.Cancel();
        base.OnDetachedFromVisualTree(e);
    }
}

public class SmoothScrollDemoModel : ModelBase
{
    public IReadOnlyList<int> Items { get; } = Enumerable.Range(0, 10000).ToArray();
    public IReadOnlyList<string> AnimationModes { get; } = ["Default", "Teleportation"];
    public IReadOnlyList<string> Children { get; } = Enumerable.Range(1, 15).Select(i => $"Child {i}").ToArray();
    public IReadOnlyList<string> Cards { get; } = Enumerable.Range(1, 6).Select(i => $"Child {i}").ToArray();
    public ObservableCollection<SmoothAnchorItem> AnchorItems { get; } = new(
        Enumerable.Range(0, 100).Select(i => new SmoothAnchorItem { Title = $"Row {i}", Height = 40 }));
    public string Text { get; set => SetField(ref field, value); } = string.Join(Environment.NewLine,
        Enumerable.Range(1, 200).Select(i => $"Line {i}: edit this text, select across lines, or move the caret with the keyboard to scroll into view."));

    public string ZoomCode => """
        // Load <ovo:OvoScrollViewTheme /> in the example's Styles.
        private void Zoom_OnClick(object? sender, RoutedEventArgs e) =>
            ReferenceZoom.ZoomTo(ReferenceZoom.ZoomFactor *
                ((sender as Button)?.Tag as string == "in" ? 1.2 : 1 / 1.2));
        """;

    public string ListCode => """
        // Constructor: InitializeComponent(); DataContext = new SmoothScrollDemoModel();
        public IReadOnlyList<int> Items { get; } = Enumerable.Range(0, 10000).ToArray();
        public IReadOnlyList<string> AnimationModes { get; } = ["Default", "Teleportation"];
        private OvoScrollPresenter? _listPresenter;
        private void List_OnLoaded(object? sender, RoutedEventArgs e)
        {
            List_OnUnloaded(sender, e);
            _listPresenter = ReferenceList.GetVisualDescendants().OfType<OvoScrollPresenter>().FirstOrDefault();
            if (_listPresenter is not null) _listPresenter.ScrollAnimationStarting += List_OnAnimation;
        }
        private void List_OnUnloaded(object? sender, RoutedEventArgs e)
        {
            if (_listPresenter is not null) _listPresenter.ScrollAnimationStarting -= List_OnAnimation;
            _listPresenter = null;
        }
        private void BringItem_OnClick(object? sender, RoutedEventArgs e)
        {
            ReferenceList.SelectedIndex = Random.Shared.Next(ReferenceList.ItemCount);
            ReferenceList.ScrollIntoView(ReferenceList.SelectedIndex);
        }
        private void List_OnAnimation(object? sender, ScrollAnimationStartingEventArgs e)
        {
            if (AnimationMode.SelectedIndex != 1) return;
            var animation = e.Animation.Compositor.CreateVector3DKeyFrameAnimation();
            var delta = e.EndPosition.Y - e.StartingPosition.Y;
            animation.InsertKeyFrame(0.499999f, new Vector3D(e.StartingPosition.X, e.EndPosition.Y - 0.9 * delta, 0), new SplineEasing(1, 0, 1, 0));
            animation.InsertKeyFrame(0.5f, new Vector3D(e.StartingPosition.X, e.EndPosition.Y - 0.1 * delta, 0), new StepEasing());
            animation.InsertKeyFrame(1, new Vector3D(e.EndPosition.X, e.EndPosition.Y, 0), new SplineEasing(0, 1, 0, 1));
            animation.Duration = TimeSpan.FromMilliseconds(1500);
            e.Animation = animation;
        }
        """;

    public string TextCode => """
        // Constructor: InitializeComponent(); DataContext = new SmoothScrollDemoModel();
        // Load <ovo:OvoScrollViewerTheme /> only in this example's Styles.
        public string Text { get; set; } = string.Join(Environment.NewLine,
            Enumerable.Range(1, 200).Select(i => $"Line {i}: edit this text, select across lines, or move the caret with the keyboard to scroll into view."));
        """;

    public string ChainingCode => """
        // Constructor: InitializeComponent(); DataContext = new SmoothScrollDemoModel();
        public IReadOnlyList<string> Children { get; } = Enumerable.Range(1, 15).Select(i => $"Child {i}").ToArray();
        public IReadOnlyList<string> Cards { get; } = Enumerable.Range(1, 6).Select(i => $"Child {i}").ToArray();
        // The ToggleSwitch binds IsScrollChainingEnabled on both inner viewers.
        // ContentOrientation is set on the horizontal viewer's OvoScrollPresenter.
        """;

    public string AnchorCode => """
        // Constructor: InitializeComponent(); DataContext = new SmoothScrollDemoModel();
        public ObservableCollection<SmoothAnchorItem> AnchorItems { get; } = new(
            Enumerable.Range(0, 100).Select(i => new SmoothAnchorItem { Title = $"Row {i}", Height = 40 }));
        public class SmoothAnchorItem : ModelBase
        {
            public string Title { get; init; } = "Row";
            public double Height { get; set => SetField(ref field, value); } = 40;
        }
        // The item template's Border uses Loaded / Unloaded, after visual attachment.
        private void Anchor_Loaded(object? sender, RoutedEventArgs e)
        {
            if (sender is Control element && ReferenceAnchors.IsVisualAncestorOf(element))
                ReferenceAnchors.RegisterAnchorCandidate(element);
        }
        private void Anchor_Unloaded(object? sender, RoutedEventArgs e)
        {
            if (sender is Control element)
                ReferenceAnchors.UnregisterAnchorCandidate(element);
        }
        private int _scrollChanged;
        private void Anchors_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            _scrollChanged++;
            ReferenceAnchorStatus.Text = $"Anchor: {(ReferenceAnchors.CurrentAnchor?.DataContext as SmoothAnchorItem)?.Title ?? "none"} · ScrollChanged: {_scrollChanged}";
        }
        private CancellationTokenSource? _resizeCancellation;
        private async void Resize_OnClick(object? sender, RoutedEventArgs e)
        {
            if (_resizeCancellation is not null) return;
            using var cancellation = new CancellationTokenSource();
            _resizeCancellation = cancellation;
            ReferenceResize.IsEnabled = false;
            try
            {
                foreach (var row in ((SmoothScrollDemoModel)DataContext!).AnchorItems)
                {
                    cancellation.Token.ThrowIfCancellationRequested();
                    row.Height = row.Height == 40 ? 80 : 40;
                    await Task.Delay(5, cancellation.Token);
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
            finally { _resizeCancellation = null; ReferenceResize.IsEnabled = true; }
        }
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _resizeCancellation?.Cancel();
            base.OnDetachedFromVisualTree(e);
        }
        """;
}
