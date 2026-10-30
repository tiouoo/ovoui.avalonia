using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OvoUi.Demo.Classic.Views;

namespace OvoUi.Demo.Classic.Pages;

public partial class WindowCompositionMaterialPage : UserControl
{
    public WindowCompositionMaterialPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public bool IsWindows { get; } = OperatingSystem.IsWindows();

    public string SourceCode { get; } = """
                                        private void OpenMaterialWindow(object? sender, RoutedEventArgs e)
                                        {
                                            if (!OperatingSystem.IsWindows())
                                                return;

                                            new CompositionMaterialWindow().Show();
                                        }
                                        """;

    private void OpenMaterialWindow(object? sender, RoutedEventArgs e)
    {
        if (!IsWindows)
            return;

        new CompositionMaterialWindow().Show();
    }
}