using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace OvoUi.Demo.Pages;

public partial class RefreshContainerPage : UserControl, INotifyPropertyChanged
{
    private int _nextId = 6;
    private string _status = "Ready to refresh";

    public RefreshContainerPage()
    {
        InitializeComponent();
        Items = new ObservableCollection<ActivityItem>(Enumerable.Range(1, 5).Select(CreateItem));
        DataContext = this;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ActivityItem> Items { get; }

    public string Status
    {
        get => _status;
        private set
        {
            if (_status == value) return;
            _status = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
    }

    public string SourceCode { get; } = """
                                        public ObservableCollection<ActivityItem> Items { get; } = new();

                                        private async void OnRefreshRequested(object? sender, RefreshRequestedEventArgs e)
                                        {
                                            var deferral = e.GetDeferral();
                                            try
                                            {
                                                Status = "Refreshing...";
                                                await Task.Delay(900);
                                                Items.Insert(0, CreateItem(_nextId++));
                                                Status = "Refresh completed";
                                            }
                                            finally
                                            {
                                                deferral.Complete();
                                            }
                                        }
                                        """;

    private async void OnRefreshRequested(object? sender, RefreshRequestedEventArgs e)
    {
        var deferral = e.GetDeferral();
        try
        {
            Status = "Refreshing...";
            await Task.Delay(900);
            Items.Insert(0, CreateItem(_nextId++));
            Status = $"Refresh completed. {Items.Count} items loaded.";
        }
        finally
        {
            deferral.Complete();
        }
    }

    private static ActivityItem CreateItem(int id) =>
        new(id, $"Activity {id}", $"Received at {DateTime.Now:HH:mm:ss}");
}

public sealed record ActivityItem(int Id, string Title, string Detail);