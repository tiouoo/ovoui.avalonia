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

public partial class SmoothScrollViewerPage : UserControl
{
    private OvoScrollPresenter? _listPresenter;
    private SmoothScrollViewerDemoModel Model => (SmoothScrollViewerDemoModel)DataContext!;

    public SmoothScrollViewerPage()
    {
        InitializeComponent();
        DataContext = new SmoothScrollViewerDemoModel();
    }

    private void VirtualList_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _listPresenter = VirtualList.GetVisualDescendants().OfType<OvoScrollPresenter>().FirstOrDefault();
        if (_listPresenter is not null)
            _listPresenter.ScrollAnimationStarting += ScrollAnimationStarting;
    }

    private void VirtualList_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (_listPresenter is not null)
            _listPresenter.ScrollAnimationStarting -= ScrollAnimationStarting;
        _listPresenter = null;
    }

    private void BringItem_OnClick(object? sender, RoutedEventArgs e)
    {
        VirtualList.SelectedIndex = Random.Shared.Next(VirtualList.ItemCount);
        VirtualList.ScrollIntoView(VirtualList.SelectedIndex);
    }

    private void ScrollAnimationStarting(object? sender, ScrollAnimationStartingEventArgs e)
    {
        if (CustomAnimation.IsChecked != true)
            return;
        var animation = e.Animation.Compositor.CreateVector3DKeyFrameAnimation();
        var delta = e.EndPosition.Y - e.StartingPosition.Y;
        animation.InsertKeyFrame(0.499999f, new Vector3D(e.EndPosition.X, e.EndPosition.Y - 0.9 * delta, 0), new CubicEaseIn());
        animation.InsertKeyFrame(0.5f, new Vector3D(e.EndPosition.X, e.EndPosition.Y - 0.1 * delta, 0), new StepEasing());
        animation.InsertKeyFrame(1, new Vector3D(e.EndPosition.X, e.EndPosition.Y, 0), new CubicEaseOut());
        animation.Duration = TimeSpan.FromMilliseconds(800);
        e.Animation = animation;
    }

    private void Anchor_Loaded(object? sender, RoutedEventArgs e)
    {
        // ContainerPrepared can run before the item joins the presenter's visual tree.
        if (sender is Control element && AnchorViewer.IsVisualAncestorOf(element))
            AnchorViewer.RegisterAnchorCandidate(element);
    }

    private void Anchor_Unloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is Control element)
            AnchorViewer.UnregisterAnchorCandidate(element);
    }
    private void InsertAnchor_OnClick(object? sender, RoutedEventArgs e) => Model.AnchorItems.Insert(0, new SmoothAnchorItem { Title = $"Inserted row {++Model.Inserted}" });
    private void RemoveAnchor_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Model.AnchorItems.Count > 0)
            Model.AnchorItems.RemoveAt(0);
    }

    private void ResizeAnchors_OnClick(object? sender, RoutedEventArgs e)
    {
        foreach (var row in Model.AnchorItems)
            row.Height = row.Height == 56 ? 92 : 56;
    }

    private void AnchorViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (AnchorStatus is not null)
            AnchorStatus.Text = $"Anchor: {(AnchorViewer.CurrentAnchor?.DataContext as SmoothAnchorItem)?.Title ?? "none"}";
    }
}

public class SmoothAnchorItem : ModelBase
{
    public string Title { get; init; } = "Row";
    public double Height { get; set => SetField(ref field, value); } = 56;
}

public class SmoothScrollViewerDemoModel : ModelBase
{
    public IReadOnlyList<string> Items { get; } = Enumerable.Range(1, 10000).Select(i => $"Smooth item {i:N0}").ToArray();
    public ObservableCollection<SmoothAnchorItem> AnchorItems { get; } = new(
        Enumerable.Range(1, 60).Select(i => new SmoothAnchorItem { Title = $"Row {i}" }));
    public int Inserted { get; set; }
    public string Text { get; set => SetField(ref field, value); } = string.Join(Environment.NewLine,
        Enumerable.Range(1, 80).Select(i => $"Line {i}: smooth scrolling keeps editing, selection and caret navigation available."));

    public string ListCode => """
        InitializeComponent();
        DataContext = new SmoothScrollViewerDemoModel();

        public IReadOnlyList<string> Items { get; } = Enumerable.Range(1, 10000)
            .Select(i => $"Smooth item {i:N0}").ToArray();

        private OvoScrollPresenter? _listPresenter;
        private void VirtualList_OnLoaded(object? sender, RoutedEventArgs e)
        {
            _listPresenter = VirtualList.GetVisualDescendants().OfType<OvoScrollPresenter>().FirstOrDefault();
            if (_listPresenter is not null)
                _listPresenter.ScrollAnimationStarting += ScrollAnimationStarting;
        }
        private void VirtualList_OnUnloaded(object? sender, RoutedEventArgs e)
        {
            if (_listPresenter is not null)
                _listPresenter.ScrollAnimationStarting -= ScrollAnimationStarting;
            _listPresenter = null;
        }
        private void ScrollAnimationStarting(object? sender, ScrollAnimationStartingEventArgs e)
        {
            if (CustomAnimation.IsChecked != true) return;
            var animation = e.Animation.Compositor.CreateVector3DKeyFrameAnimation();
            var delta = e.EndPosition.Y - e.StartingPosition.Y;
            animation.InsertKeyFrame(0.499999f, new Vector3D(e.EndPosition.X, e.EndPosition.Y - 0.9 * delta, 0), new CubicEaseIn());
            animation.InsertKeyFrame(0.5f, new Vector3D(e.EndPosition.X, e.EndPosition.Y - 0.1 * delta, 0), new StepEasing());
            animation.InsertKeyFrame(1, new Vector3D(e.EndPosition.X, e.EndPosition.Y, 0), new CubicEaseOut());
            animation.Duration = TimeSpan.FromMilliseconds(800);
            e.Animation = animation;
        }
        private void BringItem_OnClick(object? sender, RoutedEventArgs e)
        {
            VirtualList.SelectedIndex = Random.Shared.Next(VirtualList.ItemCount);
            VirtualList.ScrollIntoView(VirtualList.SelectedIndex);
        }
        """;

    public string TextCode => """
        InitializeComponent();
        DataContext = new SmoothScrollViewerDemoModel();
        public string Text { get; set; } = string.Join(Environment.NewLine,
            Enumerable.Range(1, 80).Select(i => $"Line {i}: smooth scrolling keeps editing, selection and caret navigation available."));
        // Add <ovo:OvoScrollViewerTheme /> to this page's UserControl.Styles.
        """;

    public string AnchorCode => """
        InitializeComponent();
        DataContext = new SmoothScrollViewerDemoModel();
        public ObservableCollection<SmoothAnchorItem> AnchorItems { get; } = new(
            Enumerable.Range(1, 60).Select(i => new SmoothAnchorItem { Title = $"Row {i}" }));
        public class SmoothAnchorItem : ModelBase
        {
            public string Title { get; init; } = "Row";
            public double Height { get; set => SetField(ref field, value); } = 56;
        }
        // The item template's Border uses Loaded / Unloaded, after visual attachment.
        private void Anchor_Loaded(object? sender, RoutedEventArgs e)
        {
            if (sender is Control element && AnchorViewer.IsVisualAncestorOf(element))
                AnchorViewer.RegisterAnchorCandidate(element);
        }
        private void Anchor_Unloaded(object? sender, RoutedEventArgs e)
        {
            if (sender is Control element)
                AnchorViewer.UnregisterAnchorCandidate(element);
        }
        private SmoothScrollViewerDemoModel Model => (SmoothScrollViewerDemoModel)DataContext!;
        private void InsertAnchor_OnClick(object? sender, RoutedEventArgs e) =>
            Model.AnchorItems.Insert(0, new SmoothAnchorItem { Title = "Inserted row" });
        private void RemoveAnchor_OnClick(object? sender, RoutedEventArgs e)
        {
            if (Model.AnchorItems.Count > 0) Model.AnchorItems.RemoveAt(0);
        }
        private void ResizeAnchors_OnClick(object? sender, RoutedEventArgs e)
        {
            foreach (var row in Model.AnchorItems) row.Height = row.Height == 56 ? 92 : 56;
        }
        private void AnchorViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e) =>
            AnchorStatus.Text = $"Anchor: {(AnchorViewer.CurrentAnchor?.DataContext as SmoothAnchorItem)?.Title}";
        """;
}
