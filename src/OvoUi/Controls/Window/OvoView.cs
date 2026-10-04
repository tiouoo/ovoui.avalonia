using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace OvoUi.Controls;

public class OvoView : ContentControl
{
    private OverlayDialogHost? _dialogHost;
    public const string PART_DialogHost = "PART_DialogHost";
    protected override Type StyleKeyOverride => typeof(OvoView);
    
    public string HostId { get; set; } = Guid.NewGuid().ToString();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_dialogHost is not null) LogicalChildren.Remove(_dialogHost);
        _dialogHost = e.NameScope.Find<OverlayDialogHost>(PART_DialogHost);
        if (_dialogHost is not null) LogicalChildren.Add(_dialogHost);
    }
}
