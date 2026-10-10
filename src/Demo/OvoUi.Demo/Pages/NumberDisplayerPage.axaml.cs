using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Controls;
using OvoUi.Demo.Models;

namespace OvoUi.Demo.Pages;

public partial class NumberDisplayerPage : UserControl
{
    public NumberDisplayerPage()
    {
        InitializeComponent();
        DataContext = new NumberDisplayerDemoViewModel();
    }
}

public sealed class NumberDisplayerDemoViewModel : INotifyPropertyChanged
{
    private readonly Random _random = new();
    private int _value = 1280;
    private long _longValue = 250000;
    private double _doubleValue = 42.5;
    private DateTime _dateValue = DateTime.Today;

    public event PropertyChangedEventHandler? PropertyChanged;
    public int Value { get => _value; private set => Set(ref _value, value); }
    public long LongValue { get => _longValue; private set => Set(ref _longValue, value); }
    public double DoubleValue { get => _doubleValue; private set => Set(ref _doubleValue, value); }
    public DateTime DateValue { get => _dateValue; private set => Set(ref _dateValue, value); }
    public ICommand ChangeCommand { get; }
    public string SourceCode { get; } = """
        private void ChangeValues()
        {
            Value = _random.Next(1000, 1000000);
            LongValue = _random.NextInt64(100000, 100000000);
            DoubleValue = _random.NextDouble() * 100000;
            DateValue = DateTime.Today.AddDays(_random.Next(-30, 365));
        }
        """;

    public NumberDisplayerDemoViewModel() => ChangeCommand = new ActionCommand(ChangeValues);

    private void ChangeValues()
    {
        Value = _random.Next(1000, 1000000);
        LongValue = _random.NextInt64(100000, 100000000);
        DoubleValue = _random.NextDouble() * 100000;
        DateValue = DateTime.Today.AddDays(_random.Next(-30, 365));
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

