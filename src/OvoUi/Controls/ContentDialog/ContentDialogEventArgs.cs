using Avalonia.Threading;

namespace OvoUi.Controls;

public class ContentDialogButtonClickEventArgs : EventArgs
{
    private Action? _completed;
    private int _deferrals;

    public bool Cancel { get; set; }

    public Deferral GetDeferral()
    {
        _deferrals++;
        return new Deferral(CompleteDeferral);
    }

    internal void Begin(Action completed)
    {
        _completed = completed;
        _deferrals++;
    }

    internal void CompleteDeferral()
    {
        Dispatcher.UIThread.VerifyAccess();
        if (--_deferrals == 0)
            Interlocked.Exchange(ref _completed, null)?.Invoke();
    }
}

public class ContentDialogClosingEventArgs(ContentDialogResult result) : EventArgs
{
    private Action? _completed;
    private int _deferrals;

    public ContentDialogResult Result { get; } = result;
    public bool Cancel { get; set; }

    public Deferral GetDeferral()
    {
        _deferrals++;
        return new Deferral(CompleteDeferral);
    }

    internal void Begin(Action completed)
    {
        _completed = completed;
        _deferrals++;
    }

    internal void CompleteDeferral()
    {
        Dispatcher.UIThread.VerifyAccess();
        if (--_deferrals == 0)
            Interlocked.Exchange(ref _completed, null)?.Invoke();
    }
}

public class ContentDialogClosedEventArgs(ContentDialogResult result) : EventArgs
{
    public ContentDialogResult Result { get; } = result;
}
