using System.Collections.Generic;
using Avalonia.Controls;

namespace OvoUi.Demo.Pages;

public partial class TreeViewPage : UserControl
{
    public TreeViewPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public IReadOnlyList<TreeNode> Nodes { get; } =
        [new("Workspace", [new("src"), new("tests"), new("docs")]), new("Packages", [new("Avalonia"), new("OvoUi")])];

    public string SourceCode { get; } = """
                                        public IReadOnlyList<TreeNode> Nodes { get; } =
                                            [new("Workspace", [new("src"), new("tests")])];

                                        public record TreeNode(string Name, IReadOnlyList<TreeNode>? Children = null);
                                        """;
}

public record TreeNode(string Name, IReadOnlyList<TreeNode>? Children = null);