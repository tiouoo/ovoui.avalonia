using System.Windows.Input;
using AvaloniaEdit;

namespace OvoUi.AvaloniaEdit.Controls;

/// <summary>
/// AvaloniaEdit editor with bindable commands used by the OvoUi context menu.
/// </summary>
public class OvoTextEditor : TextEditor
{
    private readonly EditorCommand[] _commands;

    public OvoTextEditor()
    {
        Options.EnableHyperlinks = false;
        Options.EnableEmailHyperlinks = false;

        SelectAllCommand = new EditorCommand(SelectAll, () => CanSelectAll);
        CutCommand = new EditorCommand(Cut, () => CanCut);
        CopyCommand = new EditorCommand(Copy, () => CanCopy);
        PasteCommand = new EditorCommand(Paste, () => CanPaste);
        UndoCommand = new EditorCommand(() => Undo(), () => CanUndo);
        RedoCommand = new EditorCommand(() => Redo(), () => CanRedo);

        _commands =
        [
            (EditorCommand)SelectAllCommand,
            (EditorCommand)CutCommand,
            (EditorCommand)CopyCommand,
            (EditorCommand)PasteCommand,
            (EditorCommand)UndoCommand,
            (EditorCommand)RedoCommand
        ];

        TextArea.SelectionChanged += (_, _) => UpdateCommandStates();
        TextChanged += (_, _) => UpdateCommandStates();
    }

    public ICommand SelectAllCommand { get; }
    public ICommand CutCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand PasteCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }

    protected override Type StyleKeyOverride { get; } = typeof(TextEditor);

    private void UpdateCommandStates()
    {
        foreach (var command in _commands)
            command.RaiseCanExecuteChanged();
    }
}
