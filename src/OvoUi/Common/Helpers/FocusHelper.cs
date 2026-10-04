using Avalonia;
using Avalonia.Input;

namespace OvoUi.Common.Helpers;

public class FocusHelper
{
    public static readonly AttachedProperty<bool> DialogFocusHintProperty =
        AvaloniaProperty.RegisterAttached<FocusHelper, InputElement, bool>("DialogFocusHint");

    public static void SetDialogFocusHint(InputElement obj, bool value)
    {
        obj.SetValue(DialogFocusHintProperty, value);
    }

    public static bool GetDialogFocusHint(InputElement obj)
    {
        return obj.GetValue(DialogFocusHintProperty);
    }
}