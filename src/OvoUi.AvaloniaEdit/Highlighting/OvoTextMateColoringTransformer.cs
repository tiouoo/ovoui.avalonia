using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using OvoUi.AvaloniaEdit.Controls;

namespace OvoUi.AvaloniaEdit.Highlighting;

/// <summary>
/// Uses AvaloniaEdit.TextMate's original token renderer, then restores the
/// editor typeface so TextMate themes cannot replace the configured font weight.
/// </summary>
internal sealed class OvoTextMateColoringTransformer : TextMateColoringTransformer
{
    private readonly OvoTextEditor _editor;

    public OvoTextMateColoringTransformer(OvoTextEditor editor, Action<Exception> exceptionHandler)
        : base(editor.TextArea.TextView, exceptionHandler)
    {
        _editor = editor;
    }

    protected override void TransformLine(DocumentLine line, ITextRunConstructionContext context)
    {
        // Preserve the upstream renderer's token colors, backgrounds, italics
        // and decorations. Only its per-token font weight is replaced.
        base.TransformLine(line, context);

        if (line.Length == 0)
            return;

        ChangeLinePart(line.Offset, line.EndOffset, element =>
        {
            var tokenTypeface = element.TextRunProperties.Typeface;
            element.TextRunProperties.SetTypeface(new Typeface(
                _editor.FontFamily,
                tokenTypeface.Style,
                _editor.FontWeight));
        });
    }
}
