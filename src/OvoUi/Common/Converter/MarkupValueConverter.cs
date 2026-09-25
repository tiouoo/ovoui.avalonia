using System.Globalization;
using Avalonia.Data.Converters;

namespace OvoUi.Common.Converter;

public abstract class MarkupValueConverter : IMarkupExtension<IValueConverter>, IValueConverter
{
    public virtual IValueConverter ProvideValue(IServiceProvider _) => this;
    public abstract object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

    public virtual object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public abstract class MarkupMultiValueConverter : IMarkupExtension<IMultiValueConverter>, IMultiValueConverter
{
    public virtual IMultiValueConverter ProvideValue(IServiceProvider _) => this;
    public abstract object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture);
}