using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace OvoUi.Common.Converter;

public sealed class PlacementToExpandUpConverter : IValueConverter
{
    public static PlacementToExpandUpConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is PlacementMode.Top
            or PlacementMode.TopEdgeAlignedLeft
            or PlacementMode.TopEdgeAlignedRight;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}