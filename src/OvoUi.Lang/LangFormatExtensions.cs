namespace OvoUi.Lang;

public static class LangFormatExtensions
{
    public static LangFormatBuilder CreateFormat(this LangKey formatKey)
    {
        ArgumentNullException.ThrowIfNull(formatKey);

        var format = formatKey.Manager.GetObservable(formatKey.Key)
                     ?? throw new ArgumentException(
                         $"The manager does not contain an observable for the resource key '{formatKey.Key}'.",
                         nameof(formatKey));

        return new LangFormatBuilder(formatKey.Manager, format);
    }

    public static LangFormatBuilder CreateFormat(
        this ILangManager manager,
        IObservable<string?> format)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(format);

        return new LangFormatBuilder(manager, format);
    }

    public static LangFormatBuilder CreateFormat(
        this ILangManager manager,
        string format)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(format);

        return new LangFormatBuilder(
            manager,
            LangObservableString.FromLiteral(format));
    }
}