using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace OvoUi.Controls;

public class OvoView : ContentControl
{
    public const string PART_DialogHost = "PART_DialogHost";
    protected override Type StyleKeyOverride => typeof(OvoView);
    
    public string HostId { get; set; } = Guid.NewGuid().ToString();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var host = e.NameScope.Find<OverlayDialogHost>(PART_DialogHost);
        if (host is not null) LogicalChildren.Add(host);
    }
}