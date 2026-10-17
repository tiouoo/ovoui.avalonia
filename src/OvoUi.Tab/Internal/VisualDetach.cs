using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;

namespace OvoUi.Tab.Internal;

internal static class VisualDetach
{
    public static void FromParent(object? content)
    {
        if (content is not Control control) return;

        if (control.IsKeyboardFocusWithin)
            TopLevel.GetTopLevel(control)?.Focus();

        switch (control.Parent)
        {
            case Panel panel:
                panel.Children.Remove(control);
                break;
            case ContentControl contentControl when ReferenceEquals(contentControl.Content, control):
                contentControl.Content = null;
                break;
        }

        switch (control.GetVisualParent())
        {
            case ContentPresenter presenter when ReferenceEquals(presenter.Content, control):
                presenter.Content = null;
                break;
            case Panel visualPanel when visualPanel.Children.Contains(control):
                visualPanel.Children.Remove(control);
                break;
        }
    }
}