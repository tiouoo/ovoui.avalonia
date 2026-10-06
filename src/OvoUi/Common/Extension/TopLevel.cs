using Avalonia.Controls;

namespace OvoUi.Common.Extension;

public static class TopLevelExtension
{
    public static TopLevel? TryGetTopLevel(this Control control)
    {
        return TopLevel.GetTopLevel(control);
    }
    
    public static TopLevel GetTopLevel(this Control control)
    {
        return TopLevel.GetTopLevel(control) ?? throw new InvalidOperationException("Control is not attached to a TopLevel.");
    }
}