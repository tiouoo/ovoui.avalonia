# OvoUi.AvaloniaEdit.Showcase

An optional OvoUi package for presenting a live Avalonia control preview beside its highlighted AXAML source.

## Setup

Install `OvoUi.AvaloniaEdit.Showcase`, then add the Showcase theme after the core OvoUi and AvaloniaEdit themes:

```xml
<Application.Styles>
    <ovo:OvoUiTheme />
    <ovo:OvoAvaloniaEditTheme />
    <ovo:OvoShowcaseTheme />
</Application.Styles>
```

Use a unique `SourceKey` to let the bundled source generator extract the control content automatically:

```xml
<ovo:ControlShowcase SourceKey="primary-button">
    <Button Content="Primary action" />
</ovo:ControlShowcase>
```

The generator is bundled as an analyzer in this package; it is not required by `OvoUi.AvaloniaEdit` itself.
