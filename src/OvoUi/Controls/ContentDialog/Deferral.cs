namespace OvoUi.Controls;

public sealed class Deferral
{
    private Action? _handler;

    internal Deferral(Action handler) => _handler = handler;

    public void Complete() => Interlocked.Exchange(ref _handler, null)?.Invoke();
}
