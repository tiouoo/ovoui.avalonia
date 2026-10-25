using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OvoUi.Demo.Pages;

public partial class ClockPage : UserControl
{
    private readonly ClockDemoViewModel _viewModel = new();

    public ClockPage()
    {
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) => _viewModel.Start();

    private void OnUnloaded(object? sender, RoutedEventArgs e) => _viewModel.Stop();
}

public partial class ClockDemoViewModel : ObservableObject
{
    private readonly DispatcherTimer _timer;

    [ObservableProperty] private DateTime _currentTime = DateTime.Now;

    public ClockDemoViewModel() =>
        _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal,
            (_, _) => CurrentTime = DateTime.Now);

    public string SourceCode { get; } = """
                                        public partial class ClockDemoViewModel : ObservableObject
                                        {
                                            private readonly DispatcherTimer _timer;

                                            [ObservableProperty]
                                            private DateTime _currentTime = DateTime.Now;

                                            public ClockDemoViewModel()
                                            {
                                                _timer = new DispatcherTimer(
                                                    TimeSpan.FromSeconds(1),
                                                    DispatcherPriority.Normal,
                                                    (_, _) => CurrentTime = DateTime.Now);
                                            }

                                            public void Start()
                                            {
                                                CurrentTime = DateTime.Now;
                                                _timer.Start();
                                            }

                                            public void Stop() => _timer.Stop();
                                        }
                                        """;

    public void Start()
    {
        CurrentTime = DateTime.Now;
        _timer.Start();
    }

    public void Stop() => _timer.Stop();
}