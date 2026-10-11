using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace OvoUi.Demo.Pages;

public partial class ThemeVariantScopePage : UserControl
{
    public ThemeVariantScopePage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string SourceCode { get; } = """
                                        public ThemeVariantScopePage()
                                        {
                                            InitializeComponent();
                                            DataContext = this;
                                        }

                                        private void UseDefaultTheme(object? sender, RoutedEventArgs e) =>
                                            demoScope.RequestedThemeVariant = ThemeVariant.Default;

                                        private void UseLightTheme(object? sender, RoutedEventArgs e) =>
                                            demoScope.RequestedThemeVariant = ThemeVariant.Light;

                                        private void UseDarkTheme(object? sender, RoutedEventArgs e) =>
                                            demoScope.RequestedThemeVariant = ThemeVariant.Dark;
                                        """;

    private void UseDefaultTheme(object? sender, RoutedEventArgs e) =>
        demoScope.RequestedThemeVariant = ThemeVariant.Default;

    private void UseLightTheme(object? sender, RoutedEventArgs e) =>
        demoScope.RequestedThemeVariant = ThemeVariant.Light;

    private void UseDarkTheme(object? sender, RoutedEventArgs e) =>
        demoScope.RequestedThemeVariant = ThemeVariant.Dark;
}