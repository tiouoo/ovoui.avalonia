using Avalonia.Controls;

namespace OvoUi.Tab;

/// <summary>
/// Lets tab content append application-specific entries to the standard tab menu.
/// </summary>
public interface IOvoTabMenuProvider
{
    void PopulateTabMenu(ICollection<MenuItem> items, OvoTabMenuContext context);
}

public readonly record struct OvoTabMenuContext(OvoTabHost Host, OvoTabItem Tab);