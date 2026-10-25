using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Controls;

namespace OvoUi.Demo.Pages;

public partial class MultiAutoCompleteBoxPage : UserControl
{
    public MultiAutoCompleteBoxPage()
    {
        InitializeComponent();
        DataContext = new MultiAutoCompleteBoxDemoViewModel();
    }
}

public class MultiAutoCompleteBoxDemoViewModel
{
    public IReadOnlyList<string> Items { get; } =
    [
        "Button", "ComboBox", "DataGrid", "Dialog", "Form", "IPv4Box",
        "MultiComboBox", "OvoAutoCompleteBox", "OvoNumericUpDown", "TextBox", "TreeView"
    ];

    public AvaloniaList<string> SelectedItems { get; } = ["Button", "TextBox"];

    public string SourceCode { get; } = """
                                        public partial class MultiAutoCompleteBoxPage : UserControl
                                        {
                                            public MultiAutoCompleteBoxPage()
                                            {
                                                InitializeComponent();
                                                DataContext = new MultiAutoCompleteBoxDemoViewModel();
                                            }
                                        }

                                        public class MultiAutoCompleteBoxDemoViewModel
                                        {
                                            public IReadOnlyList<string> Items { get; } =
                                            [
                                                "Button", "ComboBox", "DataGrid", "Dialog", "Form", "IPv4Box",
                                                "MultiComboBox", "OvoAutoCompleteBox", "OvoNumericUpDown", "TextBox", "TreeView"
                                            ];

                                            public AvaloniaList<string> SelectedItems { get; } = ["Button", "TextBox"];
                                        }
                                        """;
}