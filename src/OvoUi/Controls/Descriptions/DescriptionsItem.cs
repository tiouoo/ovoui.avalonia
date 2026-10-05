using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using OvoUi.Common;
using OvoUi.Common.Enums;
using OvoUi.Common.Helpers;

namespace OvoUi.Controls;

[PseudoClasses(PseudoClassName.PC_Horizontal, PseudoClassName.PC_Vertical)]
public class DescriptionsItem : LabeledContentControl
{
    public static readonly StyledProperty<Position> LabelPositionProperty =
        AvaloniaProperty.Register<DescriptionsItem, Position>(
            nameof(LabelPosition));

    public static readonly StyledProperty<ItemAlignment> ItemAlignmentProperty =
        AvaloniaProperty.Register<DescriptionsItem, ItemAlignment>(
            nameof(ItemAlignment));

    public static readonly StyledProperty<double> LabelWidthProperty =
        AvaloniaProperty.Register<DescriptionsItem, double>(
            nameof(LabelWidth));

    static DescriptionsItem()
    {
        LabelPositionProperty.Changed.AddClassHandler<DescriptionsItem, Position>((item, args) =>
            item.OnLabelPositionChanged(args));
    }

    public Position LabelPosition
    {
        get => GetValue(LabelPositionProperty);
        set => SetValue(LabelPositionProperty, value);
    }

    public ItemAlignment ItemAlignment
    {
        get => GetValue(ItemAlignmentProperty);
        set => SetValue(ItemAlignmentProperty, value);
    }

    public double LabelWidth
    {
        get => GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePositionPseudoClass(LabelPosition);
    }

    private void OnLabelPositionChanged(AvaloniaPropertyChangedEventArgs<Position> args)
    {
        UpdatePositionPseudoClass(args.GetNewValue<Position>());
    }

    private void UpdatePositionPseudoClass(Position newPosition)
    {
        PseudoClasses.Set(PseudoClassName.PC_Horizontal, newPosition is Position.Left or Position.Right);
        PseudoClasses.Set(PseudoClassName.PC_Vertical, newPosition is Position.Top or Position.Bottom);
    }
}