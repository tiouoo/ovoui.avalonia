using System.Globalization;
using Avalonia.Layout;

namespace OvoUi.Common.Converter;

public class FormContentHeightToAlignmentConverter : MarkupValueConverter
{
    public static FormContentHeightToAlignmentConverter Instance = new(32);

    public FormContentHeightToAlignmentConverter()
    {
        Threshold = 32;
    }

    // ReSharper disable once ConvertToPrimaryConstructor
    // Justification: need to keep the default constructor for XAML
    public FormContentHeightToAlignmentConverter(double threshold)
    {
        Threshold = threshold;
    }

    public double Threshold { get; set; }

    public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double d) return VerticalAlignment.Center;
        return d > Threshold ? VerticalAlignment.Top : VerticalAlignment.Center;
    }
}