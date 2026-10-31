using System.Linq;
using Avalonia.Controls;

namespace OvoUi.Demo.Pages;

public partial class DiffViewPage : UserControl
{
    public DiffViewPage()
    {
        InitializeComponent();
        var original = Enumerable.Range(1, 180)
            .Select(index => $"setting_{index:D3} = \"Original value for setting {index}; long lines demonstrate synchronized horizontal scrolling in both panes.\"")
            .ToList();
        var updated = original.ToList();
        updated[12] = "setting_013 = \"Updated value\"";
        updated[54] = "setting_055 = \"Another updated value\"";
        updated.RemoveRange(90, 4);
        updated.InsertRange(125, ["feature_enabled = true", "preview_enabled = true", "cache_enabled = true"]);
        LongDiff.LeftText = string.Join('\n', original);
        LongDiff.RightText = string.Join('\n', updated);
    }
}
