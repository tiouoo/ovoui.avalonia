namespace OvoUi.Lang;

public sealed class LangObservableString : LangObservable<string?>
{
    public static LangObservableString FromLiteral(string? value) =>
        new(string.Empty, value);

    public LangObservableString(string key, string? initialValue)
        : base(key, initialValue)
    {
    }
}