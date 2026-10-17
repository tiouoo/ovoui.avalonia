using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace OvoUi.Lang.Extensions;

internal readonly record struct CultureSignal(CultureInfo Culture);

internal sealed class CultureSignalAdapter(IObservable<CultureInfo> source) : IObservable<CultureSignal>
{
    public IDisposable Subscribe(IObserver<CultureSignal> observer) =>
        source.Subscribe(new Adapter(observer));

    private sealed class Adapter(IObserver<CultureSignal> inner) : IObserver<CultureInfo>
    {
        public void OnCompleted() { }
        public void OnError(Exception error) { }
        public void OnNext(CultureInfo value) => inner.OnNext(new CultureSignal(value));
    }
}

public sealed class FormatTranslateConverter: IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 0)
        {
            return AvaloniaProperty.UnsetValue;
        }

        if (values[0] is not string format)
        {
            return AvaloniaProperty.UnsetValue;
        }

        var formatCulture = culture;
        var argCount = values.Count - 1;
        if (argCount > 0 && values[^1] is CultureSignal signal)
        {
            formatCulture = signal.Culture;
            argCount--;
        }

        if (argCount == 0)
        {
            return string.Format(formatCulture, format);
        }

        var args = new object?[argCount];
        for (var i = 0; i < argCount; i++)
        {
            args[i] = values[i + 1];
        }

        return string.Format(formatCulture, format, args);
    }
}
