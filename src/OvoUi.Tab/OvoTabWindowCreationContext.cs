using Avalonia;

namespace OvoUi.Tab;

public readonly record struct OvoTabWindowCreationContext(
    OvoTabService Service,
    OvoTabHost SourceHost,
    OvoTabItem Tab,
    PixelPoint ScreenPosition);