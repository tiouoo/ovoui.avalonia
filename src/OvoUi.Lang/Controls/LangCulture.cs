using System.ComponentModel;
using System.Globalization;
using Avalonia.Metadata;

namespace OvoUi.Lang;

[TypeConverter(typeof(LangCultureTypeConverter))]
public class LangCulture
{
    public static readonly LangCulture InvariantCulture = new()
    {
        Culture = CultureInfo.InvariantCulture,
        DisplayName = CultureInfo.InvariantCulture.NativeName,
    };

    [Content]
    public string CultureName
    {
        get => Culture.Name;
        set => Culture = string.IsNullOrEmpty(value)
            ? CultureInfo.InvariantCulture
            : new CultureInfo(value);
    }

    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

    public string? DisplayName { get; set; }

    public string? ShortDisplayName { get; set; }

    public string DisplayText => DisplayName ?? Culture.NativeName;

    public string SelectionDisplayText => ShortDisplayName ?? DisplayText;
}

public class LangCultureTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
            return new LangCulture { CultureName = s };

        return base.ConvertFrom(context, culture, value);
    }
}