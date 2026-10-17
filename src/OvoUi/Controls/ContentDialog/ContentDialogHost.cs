using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;

namespace OvoUi.Controls;

internal sealed class ContentDialogHost : ContentControl
{
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