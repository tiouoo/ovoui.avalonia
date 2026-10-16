# OvoUi.Tab

Animated and detachable tab workspaces for Avalonia 12.

## Setup

```xml
<Application.Styles>
    <o:OvoUiTheme />
    <tab:OvoTabTheme />
</Application.Styles>
```

Use `OvoTabHost` inside any view, including Browser, Android, and iOS:

```csharp
var host = new OvoTabHost
{
    NewTabFactory = () => new OvoTabItem("Untitled", new EditorView())
};
host.AddTab(new OvoTabItem("Welcome", new WelcomeView()));
```

Use `OvoTabWindow` on desktop for keyboard shortcuts, window lifecycle, tab detaching, and cross-window transfers. Related hosts share an `OvoTabService`; replace its `WindowFactory` to construct application-specific windows.

Each `OvoTabItem` can customize its header, glyph or geometry icon, icon size and spacing, minimum/maximum width, closeability, and application metadata. Hosts expose close guards, lifecycle events, drag state, localization strings, context-menu extension points, and a configurable page transition.

Pointer dragging is enabled automatically on supported desktop platforms. It is disabled on macOS, Browser, Android, and iOS; switching, closing, menu/API ordering, and the animated content transition remain available.
