using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OvoUi.Controls;

namespace OvoUi.Common.Classes;

public class OperateButtonEntry
{
    public OperateButtonEntry(string content, Action<object> action, bool closeOnClick = false,
        bool removeOnCLoseClock = false)
    {
        Content = content;
        Action = action;
        CloseOnClick = closeOnClick;
        RemoveOnCLoseClock = removeOnCLoseClock;
        Command = new SimpleCommand(ExecuteCommand);
    }

    public object? Content { get; set; }
    public Action<object>? Action { get; set; }
    public bool OnUIThread { get; init; } = true;
    public bool CloseOnClick { get; init; }
    public bool RemoveOnCLoseClock { get; init; }

    public ICommand Command { get; }

    public void ExecuteCommand(object parameter)
    {
        if (OnUIThread)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Action?.Invoke(parameter);
                if (CloseOnClick) CloseToastCard(parameter);
            });
        }
        else
        {
            Action?.Invoke(parameter);
            if (CloseOnClick) CloseToastCard(parameter);
        }
    }

    private void CloseToastCard(object parameter)
    {
        if (parameter is not Button button) return;
        var messageCard = button.GetVisualAncestors().OfType<OvoMessageCard>().FirstOrDefault();
        if (RemoveOnCLoseClock)
            messageCard?.Close();
        else
            messageCard?.CloseWithoutRemovingFromList();
    }

    private class SimpleCommand : ICommand
    {
        private readonly Action<object> _execute;

        public SimpleCommand(Action<object> execute)
        {
            _execute = execute;
        }

#pragma warning disable CS0067
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _execute(parameter ?? new object());
        }
    }
}