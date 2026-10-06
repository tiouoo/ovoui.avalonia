using Avalonia.Controls;
using OvoUi.Demo.Models;

namespace OvoUi.Demo.Views;

public partial class DemoView : UserControl
{
    public DemoView()
    {
        InitializeComponent();
        DataContext = new DemoViewModel();
    }
}
