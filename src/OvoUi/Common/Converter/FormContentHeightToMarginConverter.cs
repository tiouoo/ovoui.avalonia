using System.Globalization;
using Avalonia;

namespace OvoUi.Common.Converter;

public class FormContentHeightToMarginConverter : MarkupValueConverter
{
    public static FormContentHeightToMarginConverter Instance = new();

    public FormContentHeightToMarginConverter()
    {
        Threshold = 32;
    }

    // ReSharper disable once ConvertToPrimaryConstructor
    // Justification: need to keep the default constructor for XAML
    public FormContentHeightToMarginConverter(double threshold)
    {
        Threshold = threshold;
    }

    public double Threshold { get; set; }

    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double d) return new Thickness(0);
        return d > Threshold ? new Thickness(0, 8, 8, 0) : new Thickness(0, 0, 8, 0);
    }
}