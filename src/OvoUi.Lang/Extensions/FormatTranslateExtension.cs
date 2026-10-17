using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;

namespace OvoUi.Lang.Extensions;

public sealed class FormatTranslateExtension : MarkupExtension
{
    private static readonly FormatTranslateConverter SharedConverter = new();

    public IMultiValueConverter? Converter { get; set; }

    public LangKey? FormatKey { get; set; }

    [Content]
    public IList<TranslateEntry> Items { get; set; } = new List<TranslateEntry>();

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var formatKey = FormatKey;
        if (formatKey is null) return new MultiBinding();

        var formatObservable = formatKey.Manager.GetObservable(formatKey.Key);
        if (formatObservable is null) return new MultiBinding();

        var bindings = new List<BindingBase> { formatObservable.ToBinding() };

        foreach (var item in Items)
        {
            if (item.Key is not null)
            {
                var observable = item.Key.Manager.GetObservable(item.Key.Key);
                if (observable is not null) bindings.Add(observable.ToBinding());
                else bindings.Add(CreateUnsetBinding());
            }
            else if (item.Binding is not null)
            {
                bindings.Add(item.Binding);
            }
            else
            {
                bindings.Add(CreateUnsetBinding());
            }
        }

        var useBuiltinConverter = Converter is null or FormatTranslateConverter;
        if (useBuiltinConverter)
        {
            bindings.Add(new CultureSignalAdapter(formatKey.Manager.CultureChanges).ToBinding());
        }

        return new MultiBinding
        {
            Converter = Converter ?? SharedConverter,
            Bindings = bindings
        };
    }

    private static BindingBase CreateUnsetBinding() =>
        new LangObservable<object?>(string.Empty, AvaloniaProperty.UnsetValue).ToBinding();
}
