using System.ComponentModel;

namespace OvoUi.Lang;

internal sealed class PropertyChange
{
    public INotifyPropertyChanged Source { get; }

    public string PropertyName { get; }

    public Func<object?> Getter { get; }

    public PropertyChange(INotifyPropertyChanged source, string propertyName, Func<object?> getter)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        PropertyName = propertyName;
        Getter = getter ?? throw new ArgumentNullException(nameof(getter));
    }
}