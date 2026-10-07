using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace OvoUi.Demo.Pages;

public partial class ScrollToButtonPage : UserControl
{
    public ScrollToButtonPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<string> Items { get; } =
        new(Enumerable.Range(1, 100).Select(index => $"Item {index:000}"));
}