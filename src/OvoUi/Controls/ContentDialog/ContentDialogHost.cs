using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;

namespace OvoUi.Controls;

internal sealed class ContentDialogHost : ContentControl
{
    internal Border? OverlayMask { get; private set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        OverlayMask = e.NameScope.Find<Border>("PART_OverlayMask");
    }

    static ContentDialogHost()
    {
        HorizontalContentAlignmentProperty.OverrideDefaultValue<ContentDialogHost>(HorizontalAlignment.Center);
        VerticalContentAlignmentProperty.OverrideDefaultValue<ContentDialogHost>(VerticalAlignment.Center);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) => e.Handled = true;
    protected override void OnPointerReleased(PointerReleasedEventArgs e) => e.Handled = true;
    protected override void OnPointerMoved(PointerEventArgs e) => e.Handled = true;
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e) => e.Handled = true;
}
