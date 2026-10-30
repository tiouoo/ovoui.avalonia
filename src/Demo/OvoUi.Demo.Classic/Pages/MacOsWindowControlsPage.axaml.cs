using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OvoUi.Demo.Classic.Views;

namespace OvoUi.Demo.Classic.Pages;

public partial class MacOsWindowControlsPage : UserControl
{
    public MacOsWindowControlsPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public bool IsMacOs { get; } = OperatingSystem.IsMacOS();

    public string SourceCode { get; } = """
                                        private void OpenEditor(object? sender, RoutedEventArgs e)
                                        {
                                            if (!OperatingSystem.IsMacOS())
                                                return;

                                            var editor = new MacOsWindowControlsDemoWindow();
                                            if (TopLevel.GetTopLevel(this) is Window owner)
                                                editor.Show(owner);
                                            else
                                                editor.Show();
                                        }
                                        """;

    private void OpenEditor(object? sender, RoutedEventArgs e)
    {
        if (!IsMacOs)
            return;

        var editor = new MacOsWindowControlsDemoWindow();
        if (TopLevel.GetTopLevel(this) is Window owner)
            editor.Show(owner);
        else
            editor.Show();
    }
}