using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace OvoUi.Controls;

public class Timeline : ItemsControl
{
    private static readonly FuncTemplate<Panel?> DefaultPanel = new((Func<Panel>)(() => new TimelinePanel()));

    public static readonly StyledProperty<BindingBase?> IconMemberBindingProperty =
        AvaloniaProperty.Register<Timeline, BindingBase?>(
            nameof(IconMemberBinding));

    public static readonly StyledProperty<BindingBase?> HeaderMemberBindingProperty =
        AvaloniaProperty.Register<Timeline, BindingBase?>(
            nameof(HeaderMemberBinding));

    public static readonly StyledProperty<BindingBase?> ContentMemberBindingProperty =
        AvaloniaProperty.Register<Timeline, BindingBase?>(
            nameof(ContentMemberBinding));


    public static readonly StyledProperty<IDataTemplate?> IconTemplateProperty =
        AvaloniaProperty.Register<Timeline, IDataTemplate?>(
            nameof(IconTemplate));

    public static readonly StyledProperty<IDataTemplate?> DescriptionTemplateProperty =
        AvaloniaProperty.Register<Timeline, IDataTemplate?>(
            nameof(DescriptionTemplate));

    public static readonly StyledProperty<BindingBase?> TimeMemberBindingProperty =
        AvaloniaProperty.Register<Timeline, BindingBase?>(
            nameof(TimeMemberBinding));

    public static readonly StyledProperty<string?> TimeFormatProperty = AvaloniaProperty.Register<Timeline, string?>(
        nameof(TimeFormat), defaultValue: "yyyy-MM-dd HH:mm:ss");


    public static readonly StyledProperty<TimelineDisplayMode> ModeProperty =
        AvaloniaProperty.Register<Timeline, TimelineDisplayMode>(
            nameof(Mode));

    static Timeline()
    {
        ItemsPanelProperty.OverrideDefaultValue<Timeline>(DefaultPanel);
        ModeProperty.Changed.AddClassHandler<Timeline, TimelineDisplayMode>((t, e) => { t.OnDisplayModeChanged(e); });
    }

    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public BindingBase? IconMemberBinding
    {
        get => GetValue(IconMemberBindingProperty);
        set => SetValue(IconMemberBindingProperty, value);
    }

    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public BindingBase? HeaderMemberBinding
    {
        get => GetValue(HeaderMemberBindingProperty);
        set => SetValue(HeaderMemberBindingProperty, value);
    }

    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public BindingBase? ContentMemberBinding
    {
        get => GetValue(ContentMemberBindingProperty);
        set => SetValue(ContentMemberBindingProperty, value);
    }

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IDataTemplate? DescriptionTemplate
    {
        get => GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
    }

    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public BindingBase? TimeMemberBinding
    {
        get => GetValue(TimeMemberBindingProperty);
        set => SetValue(TimeMemberBindingProperty, value);
    }

    public string? TimeFormat
    {
        get => GetValue(TimeFormatProperty);
        set => SetValue(TimeFormatProperty, value);
    }

    public TimelineDisplayMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    private void OnDisplayModeChanged(AvaloniaPropertyChangedEventArgs<TimelineDisplayMode> e)
    {
        if (this.ItemsPanelRoot is TimelinePanel panel)
        {
            panel.Mode = e.NewValue.Value;
            SetItemMode();
        }
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not TimelineItem;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (item is TimelineItem t) return t;
        return new TimelineItem();
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is TimelineItem t)
        {
            bool start = index == 0;
            bool end = index == ItemCount - 1;
            t.SetEnd(start, end);
            if (IconMemberBinding is not null)
            {
                t.Bind(TimelineItem.IconProperty, IconMemberBinding);
            }

            if (HeaderMemberBinding != null)
            {
                t.Bind(HeaderedContentControl.HeaderProperty, HeaderMemberBinding);
            }

            if (ContentMemberBinding != null)
            {
                t.Bind(ContentControl.ContentProperty, ContentMemberBinding);
            }

            if (TimeMemberBinding != null)
            {
                t.Bind(TimelineItem.TimeProperty, TimeMemberBinding);
            }

            t.SetIfUnset(TimelineItem.TimeFormatProperty, TimeFormat);
            t.SetIfUnset(TimelineItem.IconTemplateProperty, IconTemplate);
            t.SetIfUnset(HeaderedContentControl.HeaderTemplateProperty, ItemTemplate);
            t.SetIfUnset(ContentControl.ContentTemplateProperty, DescriptionTemplate);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var panel = this.ItemsPanelRoot as TimelinePanel;
        if (panel is not null)
        {
            panel.Mode = this.Mode;
        }

        SetItemMode();
        return base.ArrangeOverride(finalSize);
    }

    private void SetItemMode()
    {
        if (ItemsPanelRoot is TimelinePanel panel)
        {
            var items = panel.Children.OfType<TimelineItem>();
            if (Mode == TimelineDisplayMode.Left)
            {
                foreach (var item in items)
                {
                    SetIfUnset(item, TimelineItem.PositionProperty, TimelineItemPosition.Left);
                }
            }
            else if (Mode == TimelineDisplayMode.Right)
            {
                foreach (var item in items)
                {
                    SetIfUnset(item, TimelineItem.PositionProperty, TimelineItemPosition.Right);
                }
            }
            else if (Mode == TimelineDisplayMode.Center)
            {
                foreach (var item in items)
                {
                    SetIfUnset(item, TimelineItem.PositionProperty, TimelineItemPosition.Separate);
                }
            }
            else if (Mode == TimelineDisplayMode.Alternate)
            {
                var left = false;
                foreach (var item in items)
                {
                    SetIfUnset(item, TimelineItem.PositionProperty,
                        left ? TimelineItemPosition.Left : TimelineItemPosition.Right);
                    left = !left;
                }
            }
        }
    }

    private void SetIfUnset<T>(AvaloniaObject target, StyledProperty<T> property, T value)
    {
        if (!target.IsSet(property))
            target.SetCurrentValue(property, value);
    }

    internal void InvalidateContainers()
    {
        var timelineItems = this.GetVisualDescendants().OfType<TimelineItem>().ToList();
        for (var i = 0; i < timelineItems.Count; i++)
        {
            bool isFirst = i == 0;
            bool isLast = i == timelineItems.Count - 1;
            timelineItems[i].SetEnd(isFirst, isLast);
        }
    }
}