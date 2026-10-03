namespace OvoUi.Common.Classes;

public class ResultDisposable : IDisposable
{
    private readonly IDisposable? _disposable;

    public ResultDisposable(IDisposable? disposable, bool result)
    {
        _disposable = disposable;
        Result = result;
    }

    public bool Result { get; }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}

internal class EmptyDisposable : IDisposable
{
    public void Dispose()
    {
    }
}