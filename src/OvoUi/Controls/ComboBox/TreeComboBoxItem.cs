using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using OvoUi.Common.Classes;
using OvoUi.Common.Extension;
using OvoUi.Common.Helpers;

namespace OvoUi.Controls;

[TemplatePart(PartNames.PART_Header, typeof(Control))]
public class TreeComboBoxItem : HeaderedItemsControl, ISelectable
{
    public static readonly StyledProperty<bool> IsSelectedProperty =
        TreeViewItem.IsSelectedProperty.AddOwner<TreeComboBoxItem>();

    public static readonly StyledProperty<bool> IsExpandedProperty =
        TreeViewItem.IsExpandedProperty.AddOwner<TreeComboBoxItem>();

    public static readonly StyledProperty<bool> IsSelectableProperty =
        AvaloniaProperty.Register<TreeComboBoxItem, bool>(
            nameof(IsSelectable), true);


    public static readonly DirectProperty<TreeComboBoxItem, int> LevelProperty =
        AvaloniaProperty.RegisterDirect<TreeComboBoxItem, int>(
            nameof(Level), o => o.Level, (o, v) => o.Level = v);

    private int _level;
    private TreeComboBox? _treeComboBox;

    static TreeComboBoxItem()
    {
        IsSelectedProperty.AffectsPseudoClass<TreeComboBoxItem>(PseudoClassName.PC_Selected,
            SelectingItemsControl.IsSelectedChangedEvent);
        PressedMixin.Attach<TreeComboBoxItem>();
    }

    public TreeComboBox? Owner => _treeComboBox;

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public bool IsSelectable
    {
        get => GetValue(IsSelectableProperty);
        set => SetValue(IsSelectableProperty, value);
    }

    public int Level
    {
        get => _level;
        protected set => SetAndRaise(LevelProperty, ref _level, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        DoubleTappedEvent.RemoveHandler(OnDoubleTapped, this);
        e.NameScope.Find<Control>(PartNames.PART_Header);
        DoubleTappedEvent.AddHandler(OnDoubleTapped, RoutingStrategies.Tunnel, true, this);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _treeComboBox = this.FindLogicalAncestorOfType<TreeComboBox>();
        Level = CalculateDistanceFromLogicalParent<TreeComboBox>(this) - 1;
        if (this.ItemTemplate is null && this._treeComboBox?.ItemTemplate is not null)
        {
            SetCurrentValue(ItemTemplateProperty, this._treeComboBox.ItemTemplate);
        }

        if (this.ItemContainerTheme is null && this._treeComboBox?.ItemContainerTheme is not null)
        {
            SetCurrentValue(ItemContainerThemeProperty, this._treeComboBox.ItemContainerTheme);
        }
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (this.ItemCount <= 0) return;
        this.SetCurrentValue(IsExpandedProperty, !IsExpanded);
        e.Handled = true;
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return EnsureParent().NeedsContainerInternal(item, index, out recycleKey);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return EnsureParent().CreateContainerForItemInternal(item, index, recycleKey);
    }

    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        EnsureParent().ContainerForItemPreparedInternal(container, item, index);
    }

    // TODO replace with helper method from shared library. 
    private static int CalculateDistanceFromLogicalParent<T>(ILogical? logical, int @default = -1) where T : ILogical
    {
        int distance = 0;
        ILogical? parent = logical;
        while (parent is not null)
        {
            if (parent is T) return distance;
            parent = parent.LogicalParent;
            distance++;
        }

        return @default;
    }

    private TreeComboBox EnsureParent()
    {
        return this._treeComboBox ??
               throw new InvalidOperationException("TreeComboBoxItem must be a part of TreeComboBox");
    }
}