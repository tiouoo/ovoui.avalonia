using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;

namespace OvoUi.AvaloniaEdit.Controls;

/// <summary>
/// Attached settings that are forwarded by <see cref="CodeBlock" /> to its inner
/// <see cref="TextEditor" />.
/// </summary>
public sealed class AvaloniaEditor : AvaloniaObject
{
    public static readonly AttachedProperty<bool> ShowLineNumbersProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, bool>(
            "ShowLineNumbers", true);

    public static readonly AttachedProperty<bool> WordWrapProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, bool>(
            "WordWrap");

    public static readonly AttachedProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, bool>(
            "IsReadOnly", true);

    public static readonly AttachedProperty<bool> RightClickMovesCaretProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, bool>(
            "RightClickMovesCaret", true);

    public static readonly AttachedProperty<FontFamily?> FontFamilyProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, FontFamily?>(
            "FontFamily");

    public static readonly AttachedProperty<double?> FontSizeProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, double?>(
            "FontSize");

    public static readonly AttachedProperty<FontWeight?> FontWeightProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, FontWeight?>(
            "FontWeight");

    public static readonly AttachedProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, ScrollBarVisibility>(
            "HorizontalScrollBarVisibility", ScrollBarVisibility.Auto);

    public static readonly AttachedProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, ScrollBarVisibility>(
            "VerticalScrollBarVisibility", ScrollBarVisibility.Auto);

    public static readonly AttachedProperty<TextEditorOptions?> OptionsProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, TextEditorOptions?>(
            "Options");

    public static readonly AttachedProperty<bool> AllowScrollBelowDocumentProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, bool>(
            "AllowScrollBelowDocument");

    public static readonly AttachedProperty<IBrush?> SelectionBrushProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, IBrush?>(
            "SelectionBrush");

    public static readonly AttachedProperty<IBrush?> SelectionForegroundProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, IBrush?>(
            "SelectionForeground");

    public static readonly AttachedProperty<IBrush?> CaretBrushProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, IBrush?>(
            "CaretBrush");

    public static readonly AttachedProperty<IBrush?> LineNumbersForegroundProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, IBrush?>(
            "LineNumbersForeground");

    public static readonly AttachedProperty<IHighlightingDefinition?> SyntaxHighlightingProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaEditor, AvaloniaObject, IHighlightingDefinition?>(
            "SyntaxHighlighting");

    private AvaloniaEditor()
    {
    }

    public static bool GetShowLineNumbers(AvaloniaObject target) => target.GetValue(ShowLineNumbersProperty);
    public static void SetShowLineNumbers(AvaloniaObject target, bool value) => target.SetValue(ShowLineNumbersProperty, value);

    public static bool GetWordWrap(AvaloniaObject target) => target.GetValue(WordWrapProperty);
    public static void SetWordWrap(AvaloniaObject target, bool value) => target.SetValue(WordWrapProperty, value);

    public static bool GetIsReadOnly(AvaloniaObject target) => target.GetValue(IsReadOnlyProperty);
    public static void SetIsReadOnly(AvaloniaObject target, bool value) => target.SetValue(IsReadOnlyProperty, value);

    public static bool GetRightClickMovesCaret(AvaloniaObject target) => target.GetValue(RightClickMovesCaretProperty);
    public static void SetRightClickMovesCaret(AvaloniaObject target, bool value) => target.SetValue(RightClickMovesCaretProperty, value);

    public static FontFamily? GetFontFamily(AvaloniaObject target) => target.GetValue(FontFamilyProperty);
    public static void SetFontFamily(AvaloniaObject target, FontFamily? value) => target.SetValue(FontFamilyProperty, value);

    public static double? GetFontSize(AvaloniaObject target) => target.GetValue(FontSizeProperty);
    public static void SetFontSize(AvaloniaObject target, double? value) => target.SetValue(FontSizeProperty, value);

    public static FontWeight? GetFontWeight(AvaloniaObject target) => target.GetValue(FontWeightProperty);
    public static void SetFontWeight(AvaloniaObject target, FontWeight? value) => target.SetValue(FontWeightProperty, value);

    public static ScrollBarVisibility GetHorizontalScrollBarVisibility(AvaloniaObject target) =>
        target.GetValue(HorizontalScrollBarVisibilityProperty);

    public static void SetHorizontalScrollBarVisibility(AvaloniaObject target, ScrollBarVisibility value) =>
        target.SetValue(HorizontalScrollBarVisibilityProperty, value);

    public static ScrollBarVisibility GetVerticalScrollBarVisibility(AvaloniaObject target) =>
        target.GetValue(VerticalScrollBarVisibilityProperty);

    public static void SetVerticalScrollBarVisibility(AvaloniaObject target, ScrollBarVisibility value) =>
        target.SetValue(VerticalScrollBarVisibilityProperty, value);

    public static TextEditorOptions? GetOptions(AvaloniaObject target) => target.GetValue(OptionsProperty);
    public static void SetOptions(AvaloniaObject target, TextEditorOptions? value) => target.SetValue(OptionsProperty, value);

    public static bool GetAllowScrollBelowDocument(AvaloniaObject target) =>
        target.GetValue(AllowScrollBelowDocumentProperty);

    public static void SetAllowScrollBelowDocument(AvaloniaObject target, bool value) =>
        target.SetValue(AllowScrollBelowDocumentProperty, value);

    public static IBrush? GetSelectionBrush(AvaloniaObject target) => target.GetValue(SelectionBrushProperty);
    public static void SetSelectionBrush(AvaloniaObject target, IBrush? value) => target.SetValue(SelectionBrushProperty, value);

    public static IBrush? GetSelectionForeground(AvaloniaObject target) => target.GetValue(SelectionForegroundProperty);
    public static void SetSelectionForeground(AvaloniaObject target, IBrush? value) => target.SetValue(SelectionForegroundProperty, value);

    public static IBrush? GetCaretBrush(AvaloniaObject target) => target.GetValue(CaretBrushProperty);
    public static void SetCaretBrush(AvaloniaObject target, IBrush? value) => target.SetValue(CaretBrushProperty, value);

    public static IBrush? GetLineNumbersForeground(AvaloniaObject target) => target.GetValue(LineNumbersForegroundProperty);
    public static void SetLineNumbersForeground(AvaloniaObject target, IBrush? value) => target.SetValue(LineNumbersForegroundProperty, value);

    public static IHighlightingDefinition? GetSyntaxHighlighting(AvaloniaObject target) =>
        target.GetValue(SyntaxHighlightingProperty);

    public static void SetSyntaxHighlighting(AvaloniaObject target, IHighlightingDefinition? value) =>
        target.SetValue(SyntaxHighlightingProperty, value);
}
