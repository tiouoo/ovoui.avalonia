using Avalonia;
using Avalonia.Controls.Primitives;
using OvoUi.Common.Extension;
using OvoUi.Common.Helpers;
using OvoUi.Common.Interfaces;

namespace OvoUi.Controls;

public class CustomDialogControl : DialogControlBase
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var closeButtonVisible = IsCloseButtonVisible ?? DataContext is IDialogContext;
        IsHitTestVisibleProperty.SetValue(closeButtonVisible, _closeButton);
        if (!closeButtonVisible) OpacityProperty.SetValue(0, _closeButton);
    }

    public override void Close()
    {
        if (DataContext is IDialogContext context)
            context.Close();
        else
            OnElementClosing(this, null);
    }
}