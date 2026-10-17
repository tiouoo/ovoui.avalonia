namespace OvoUi.Lang;

internal sealed class BoxingObserver<T>(IObserver<object?> inner) : IObserver<T>
{
    public void OnCompleted() => inner.OnCompleted();
    public void OnError(Exception error) => inner.OnError(error);
    public void OnNext(T value) => inner.OnNext(value);
}