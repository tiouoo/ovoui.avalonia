using Avalonia.Controls.Primitives;
using OvoUi.Common.Interfaces;

namespace OvoUi.Controls;

public class CustomDrawerControl : DrawerControlBase
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_closeButton is not null)
        {
            _closeButton.IsVisible = IsCloseButtonVisible ?? DataContext is IDialogContext;
            _closeButton.IsHitTestVisible = _closeButton.IsVisible;
        }
    }

    protected internal override void AnchorAndUpdatePositionInfo()
    {
    }
}
