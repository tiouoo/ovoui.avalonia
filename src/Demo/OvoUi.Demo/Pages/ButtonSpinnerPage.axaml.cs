using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace OvoUi.Demo.Pages;

public partial class ButtonSpinnerPage : UserControl
{
    private readonly string[] _mountains =
    [
        "avalonia",
        "wpf",
        "android",
        "ios",
        "windows",
        "macos",
        "linux",
        "tv",
        "web"
    ];

    public ButtonSpinnerPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string SourceCode { get; } = """
                                        private readonly string[] _mountains =
                                        [
                                            "avalonia",
                                            "wpf",
                                            "android",
                                            "ios",
                                            "windows",
                                            "macos",
                                            "linux",
                                            "tv",
                                            "web"
                                        ];

                                        public void OnSpin(object sender, SpinEventArgs e)
                                        {
                                            var spinner = (ButtonSpinner)sender;

                                            if (spinner.Content is not TextBlock txtBox) return;
                                            var value = Array.IndexOf(_mountains, txtBox.Text);
                                            if (e.Direction == SpinDirection.Increase)
                                                value++;
                                            else
                                                value--;

                                            if (value < 0)
                                                value = _mountains.Length - 1;
                                            else if (value >= _mountains.Length)
                                                value = 0;

                                            txtBox.Text = _mountains[value];
                                        }
                                        """;

    public void OnSpin(object sender, SpinEventArgs e)
    {
        var spinner = (ButtonSpinner)sender;

        if (spinner.Content is not TextBlock txtBox) return;
        var value = Array.IndexOf(_mountains, txtBox.Text);
        if (e.Direction == SpinDirection.Increase)
            value++;
        else
            value--;

        if (value < 0)
            value = _mountains.Length - 1;
        else if (value >= _mountains.Length)
            value = 0;

        txtBox.Text = _mountains[value];
    }
}