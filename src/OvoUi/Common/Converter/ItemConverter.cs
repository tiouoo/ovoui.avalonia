using System.Collections;
using Avalonia.Data.Converters;

namespace OvoUi.Common.Converter;

public static class ItemConverter
{
    public static readonly IValueConverter ItemVisibleConverter =
        new FuncValueConverter<int?, bool>(count => count is > 1);

    public static readonly IValueConverter ItemToObjectConverter =
        new FuncValueConverter<int?, IEnumerable>(count => Enumerable.Repeat(new object(), count ?? 0));
}