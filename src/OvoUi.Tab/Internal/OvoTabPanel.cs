using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;

namespace OvoUi.Tab.Internal;

/// <summary>Arranges tab headers while keeping visual drag order separate from model order.</summary>
public sealed class OvoTabPanel : Panel
{
    private readonly Dictionary<Control, TranslateTransform> _transforms = [];
    private Dictionary<OvoTabItem, double>? _settlingPositions;
    private OvoTabItem? _draggedTab;
    private double _dragX;
    private double _grabOffset;

    internal int DropIndex { get; private set; }

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = 0d;
        var height = 0d;
        foreach (var child in Children)
        {
            child.Measure(new Size(double.PositiveInfinity, availableSize.Height));
            width += child.DesiredSize.Width;
            height = Math.Max(height, child.DesiredSize.Height);
        }
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var stale in _transforms.Keys.Where(child => !Children.Contains(child)).ToArray())
            _transforms.Remove(stale);

        var x = 0d;
        foreach (var child in Children)
        {
            child.Arrange(new Rect(x, 0, child.DesiredSize.Width, finalSize.Height));
            x += child.DesiredSize.Width;
        }

        if (_draggedTab is not null)
            UpdateLocations();
        else
        {
            foreach (var child in Children)
            {
                var transform = GetTransform(child);
                if (child.DataContext is OvoTabItem tab &&
                    _settlingPositions?.TryGetValue(tab, out var oldX) == true)
                {
                    transform.Transitions = null;
                    transform.X = oldX - child.Bounds.X;
                }
                EnableAnimation(transform);
                transform.X = 0;
                child.ZIndex = 0;
            }
            _settlingPositions = null;
        }
        return finalSize;
    }

    internal bool BeginDrag(OvoTabItem tab, Point pointer, double grabOffset)
    {
        if (!Children.Any(child => ReferenceEquals(child.DataContext, tab))) return false;
        _draggedTab = tab;
        _settlingPositions = null;
        _grabOffset = grabOffset;
        DragTo(pointer);
        return true;
    }

    internal void DragTo(Point pointer)
    {
        _dragX = pointer.X - _grabOffset;
        UpdateLocations();
    }

    internal void EndDrag()
    {
        if (_draggedTab is null) return;
        _settlingPositions = Children.Where(child => child.DataContext is OvoTabItem)
            .ToDictionary(child => (OvoTabItem)child.DataContext!,
                child => child.Bounds.X + GetTransform(child).X);
        _draggedTab = null;
        InvalidateArrange();
    }

    private void UpdateLocations()
    {
        var dragged = Children.FirstOrDefault(child => ReferenceEquals(child.DataContext, _draggedTab));
        if (dragged is null) return;
        var width = dragged.DesiredSize.Width;
        var totalWidth = Children.Sum(child => child.DesiredSize.Width);
        var dragX = Math.Clamp(_dragX, 0, Math.Max(0, totalWidth - width));
        var crossingEdge = dragX > dragged.Bounds.X ? dragX + width : dragX;
        var siblings = Children.Where(child => !ReferenceEquals(child, dragged)).ToList();
        DropIndex = siblings.Count(child => crossingEdge > child.Bounds.X + child.DesiredSize.Width / 2);
        siblings.Insert(DropIndex, dragged);

        var x = 0d;
        foreach (var child in siblings)
        {
            var transform = GetTransform(child);
            if (ReferenceEquals(child, dragged))
            {
                transform.Transitions = null;
                transform.X = dragX - child.Bounds.X;
                child.ZIndex = 1;
            }
            else
            {
                EnableAnimation(transform);
                transform.X = x - child.Bounds.X;
                child.ZIndex = 0;
            }
            x += child.DesiredSize.Width;
        }
    }

    private TranslateTransform GetTransform(Control child)
    {
        if (_transforms.TryGetValue(child, out var transform)) return transform;
        transform = new TranslateTransform();
        child.RenderTransform = transform;
        _transforms.Add(child, transform);
        return transform;
    }

    private static void EnableAnimation(TranslateTransform transform) => transform.Transitions ??= new Transitions
    {
        new DoubleTransition
        {
            Property = TranslateTransform.XProperty,
            Duration = TimeSpan.FromMilliseconds(160),
            Easing = new CubicEaseOut()
        }
    };
}
