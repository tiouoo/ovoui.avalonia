using System;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace OvoUi.Lang.Extensions;

public sealed class TranslateExtension : MarkupExtension
{
    public LangKey? Key { get; set; }

    public TranslateExtension()
    {
    }

    public TranslateExtension(LangKey key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var observable = Key?.Manager.GetObservable(Key.Key);
        if (observable is null) return AvaloniaProperty.UnsetValue;
        return observable.ToBinding();
    }
}