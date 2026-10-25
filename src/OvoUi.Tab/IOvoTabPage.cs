using System.Threading;
using System.Threading.Tasks;

namespace OvoUi.Tab;

/// <summary>
/// Optional lifecycle contract for content hosted by an <see cref="OvoTabItem"/>.
/// </summary>
public interface IOvoTabPage
{
    ValueTask<bool> OnClosingAsync(OvoTabClosingContext context, CancellationToken cancellationToken) =>
        ValueTask.FromResult(true);

    void OnClosed(OvoTabClosedContext context)
    {
    }
}

public readonly record struct OvoTabClosingContext(OvoTabItem Tab, OvoTabCloseReason Reason);

public readonly record struct OvoTabClosedContext(OvoTabItem Tab, OvoTabCloseReason Reason);

public enum OvoTabCloseReason
{
    User,
    CloseOthers,
    WindowClosing,
    Programmatic
}