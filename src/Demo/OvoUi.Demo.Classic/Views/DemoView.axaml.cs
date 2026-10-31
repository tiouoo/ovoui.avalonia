using Avalonia.Controls;
using OvoUi.Demo.Classic.Models;

namespace OvoUi.Demo.Classic.Views;

public partial class DemoView : UserControl
{
    public DemoView()
    {
        InitializeComponent();
        DataContext = new DemoViewModel();
    }
}