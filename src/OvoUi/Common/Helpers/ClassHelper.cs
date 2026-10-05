using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Collections;

namespace OvoUi.Common.Helpers;

public class ClassHelper
{
    public static readonly AttachedProperty<object?> ClassesProperty =
        AvaloniaProperty.RegisterAttached<ClassHelper, StyledElement, object?>("Classes");

    public static readonly AttachedProperty<StyledElement> ClassSourceProperty =
        AvaloniaProperty.RegisterAttached<ClassHelper, StyledElement, StyledElement>("ClassSource");

    private static readonly ConditionalWeakTable<StyledElement, ForwardedClassesState> ForwardedClasses = new();

    static ClassHelper()
    {
        ClassesProperty.Changed.AddClassHandler<StyledElement>(OnClassesChanged);
        ClassSourceProperty.Changed.AddClassHandler<StyledElement>(OnClassSourceChanged);
    }

    private static void OnClassSourceChanged(StyledElement arg1, AvaloniaPropertyChangedEventArgs arg2)
    {
        if (arg2.NewValue is not StyledElement styledElement) return;
        arg1.Classes.Clear();
        var nonPseudoClasses = styledElement.Classes.Where(c => !c.StartsWith(":"));
        arg1.Classes.AddRange(nonPseudoClasses);
        styledElement.Classes.WeakSubscribe((o, e) => OnSourceClassesChanged(o!, e, arg1));
    }

    private static void OnSourceClassesChanged(object sender, NotifyCollectionChangedEventArgs e, StyledElement target)
    {
        if (sender is not AvaloniaList<string> classes) return;
        target.Classes.Clear();
        var nonPseudoClasses = classes.Where(c => !c.StartsWith(":"));
        target.Classes.AddRange(nonPseudoClasses);
    }

    public static void SetClasses(AvaloniaObject obj, object? value)
    {
        obj.SetValue(ClassesProperty, value);
    }

    public static object? GetClasses(AvaloniaObject obj)
    {
        return obj.GetValue(ClassesProperty);
    }

    private static void OnClassesChanged(StyledElement sender, AvaloniaPropertyChangedEventArgs value)
    {
        var state = ForwardedClasses.GetOrCreateValue(sender);
        state.Subscription?.Dispose();
        state.Subscription = null;

        ApplyClasses(sender, state, value.NewValue);

        if (value.NewValue is INotifyCollectionChanged source)
        {
            state.Subscription = source.WeakSubscribe((_, _) =>
                ApplyClasses(sender, state, sender.GetValue(ClassesProperty)));
        }
    }

    private static void ApplyClasses(StyledElement target, ForwardedClassesState state, object? value)
    {
        foreach (var className in state.AppliedClasses)
        {
            target.Classes.Remove(className);
        }

        state.AppliedClasses.Clear();

        foreach (var className in EnumerateClasses(value).Distinct(StringComparer.Ordinal))
        {
            if (className.StartsWith(':') || target.Classes.Contains(className)) continue;
            target.Classes.Add(className);
            state.AppliedClasses.Add(className);
        }
    }

    private static IEnumerable<string> EnumerateClasses(object? value)
    {
        return value switch
        {
            string classes => SplitClasses(classes),
            IEnumerable<string> classes => classes.SelectMany(SplitClasses),
            _ => []
        };
    }

    private static IEnumerable<string> SplitClasses(string classes)
    {
        return classes.Split((char[]?)null,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private sealed class ForwardedClassesState
    {
        public HashSet<string> AppliedClasses { get; } = [];
        public IDisposable? Subscription { get; set; }
    }

    public static void SetClassSource(StyledElement obj, StyledElement value)
    {
        obj.SetValue(ClassSourceProperty, value);
    }

    public static StyledElement GetClassSource(StyledElement obj)
    {
        return obj.GetValue(ClassSourceProperty);
    }
}
