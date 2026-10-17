namespace OvoUi.SmoothScroll.Controls;

/// <summary>
/// Defines constants that specify the orientation of content scrolling in a <see cref = "OvoScrollView"/>.
/// </summary>
[Flags]
public enum ScrollContentOrientation
{
    None = 0,
    Vertical = 1,
    Horizontal = 2,
    Both = Vertical | Horizontal
}