using Avalonia.Controls;

namespace OvoUi.Demo.Pages;

public partial class OvoAutoCompleteBoxPage : UserControl
{
    public OvoAutoCompleteBoxPage()
    {
        InitializeComponent();
        DataContext = new OvoAutoCompleteBoxDemoViewModel();
    }
}

public class OvoAutoCompleteBoxDemoViewModel
{
    public IReadOnlyList<string> Controls { get; } =
    [
        "Button", "ComboBox", "DataGrid", "Dialog", "Form", "IPv4Box",
        "MultiComboBox", "OvoAutoCompleteBox", "OvoNumericUpDown", "TreeView"
    ];

    public string SourceCode { get; } = """
        public partial class OvoAutoCompleteBoxPage : UserControl
        {
            public OvoAutoCompleteBoxPage()
            {
                InitializeComponent();
                DataContext = new OvoAutoCompleteBoxDemoViewModel();
            }
        }

        public class OvoAutoCompleteBoxDemoViewModel
        {
            public IReadOnlyList<string> Controls { get; } =
            [
                "Button", "ComboBox", "DataGrid", "Dialog", "Form", "IPv4Box",
                "MultiComboBox", "OvoAutoCompleteBox", "OvoNumericUpDown", "TreeView"
            ];
        }
        """;
}
