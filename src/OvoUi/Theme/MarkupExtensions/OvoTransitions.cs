using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Markup.Xaml;

namespace OvoUi.Theme.MarkupExtensions;

public abstract class OvoTransitionExtension : MarkupExtension
{
    protected abstract IEnumerable<AvaloniaProperty> Properties { get; }

    protected virtual TimeSpan Duration =>
        TimeSpan.FromMilliseconds(100);

    private static readonly Dictionary<Type, Func<TimeSpan, AvaloniaProperty, ITransition>> TransitionFactories =
        new()
        {
            {
                typeof(IBrush),
                (duration, property) => new BrushTransition
                {
                    Duration = duration,
                    Property = property
                }
            },
            {
                typeof(BoxShadows),
                (duration, property) => new BoxShadowsTransition
                {
                    Duration = duration,
                    Property = property
                }
            }
        };

    protected ITransition CreateTransition(AvaloniaProperty property)
    {
        var propertyType = property.PropertyType;

        foreach (var factory in TransitionFactories)
        {
            if (factory.Key.IsAssignableFrom(propertyType))
            {
                return factory.Value(Duration, property);
            }
        }

        throw new NotSupportedException(
            $"OvoTransition does not support property '{property.Name}' " +
            $"with type '{propertyType.Name}'.");
    }

    private Transitions CreateTransitions()
    {
        var transitions = new Transitions();

        foreach (var property in Properties)
        {
            transitions.Add(CreateTransition(property));
        }

        return transitions;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return CreateTransitions();
    }
}

public sealed class OvoTransitions : OvoTransitionExtension
{
    protected override IEnumerable<AvaloniaProperty> Properties =>
    [
        Border.BackgroundProperty,
        TextElement.ForegroundProperty,
        Border.BorderBrushProperty,
        Border.BoxShadowProperty
    ];
}

public sealed class OvoBackgroundTransition : OvoTransitionExtension
{
    protected override IEnumerable<AvaloniaProperty> Properties =>
    [
        Border.BackgroundProperty
    ];

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return CreateTransition(Border.BackgroundProperty);
    }
}

public sealed class OvoBoxShadowTransition : OvoTransitionExtension
{
    protected override IEnumerable<AvaloniaProperty> Properties =>
    [
        Border.BoxShadowProperty
    ];

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return CreateTransition(Border.BoxShadowProperty);
    }
}

public sealed class OvoForegroundTransition : OvoTransitionExtension
{
    protected override IEnumerable<AvaloniaProperty> Properties =>
    [
        TextElement.ForegroundProperty
    ];

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return CreateTransition(TextElement.ForegroundProperty);
    }
}

public sealed class OvoBorderTransition : OvoTransitionExtension
{
    protected override IEnumerable<AvaloniaProperty> Properties =>
    [
        Border.BorderBrushProperty
    ];

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return CreateTransition(Border.BorderBrushProperty);
    }
}