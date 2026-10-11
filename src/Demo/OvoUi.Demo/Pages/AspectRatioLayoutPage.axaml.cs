using Avalonia.Controls;
using Avalonia.Interactivity;

namespace OvoUi.Demo.Pages;

public partial class AspectRatioLayoutPage : UserControl
{
    public AspectRatioLayoutPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string SourceCode { get; } = """
                                        private void UseHorizontalPreset(object? sender, RoutedEventArgs e)
                                        {
                                            widthSlider.Value = 640;
                                            heightSlider.Value = 260;
                                        }

                                        private void UseSquarePreset(object? sender, RoutedEventArgs e)
                                        {
                                            widthSlider.Value = 340;
                                            heightSlider.Value = 340;
                                        }

                                        private void UseVerticalPreset(object? sender, RoutedEventArgs e)
                                        {
                                            widthSlider.Value = 220;
                                            heightSlider.Value = 440;
                                        }
                                        """;

    private void UseHorizontalPreset(object? sender, RoutedEventArgs e)
    {
        widthSlider.Value = 640;
        heightSlider.Value = 260;
    }

    private void UseSquarePreset(object? sender, RoutedEventArgs e)
    {
        widthSlider.Value = 340;
        heightSlider.Value = 340;
    }

    private void UseVerticalPreset(object? sender, RoutedEventArgs e)
    {
        widthSlider.Value = 220;
        heightSlider.Value = 440;
    }
}