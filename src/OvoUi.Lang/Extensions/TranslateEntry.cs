using Avalonia.Data;

namespace OvoUi.Lang.Extensions;

public class TranslateEntry
{
    public LangKey? Key { get; set; }

    [AssignBinding] public BindingBase? Binding { get; set; }
}