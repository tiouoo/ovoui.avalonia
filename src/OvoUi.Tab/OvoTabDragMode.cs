namespace OvoUi.Tab;

/// <summary>
/// Controls pointer-based tab manipulation. Automatic enables full dragging on supported desktop platforms only.
/// </summary>
public enum OvoTabDragMode
{
    Automatic,
    Disabled,
    ReorderOnly,
    ReorderAndDetach
}