using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
        CutCommand = new EditorCommand(Cut, () => !IsReadOnly && CanCut);
        CopyCommand = new EditorCommand(Copy, () => CanCopy);
        PasteCommand = new EditorCommand(Paste, () => !IsReadOnly && CanPaste);
        UndoCommand = new EditorCommand(() =>
        {
            Undo();
            UpdateCommandStates();
        }, () => !IsReadOnly && CanUndo);
        RedoCommand = new EditorCommand(() =>
        {
            Redo();
            UpdateCommandStates();
        }, () => !IsReadOnly && CanRedo);

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
        PropertyChanged += (_, e) =>
        {
            if (e.Property == IsReadOnlyProperty)
                UpdateCommandStates();
        };
        AddHandler(ContextRequestedEvent, (_, _) => UpdateCommandStates(), RoutingStrategies.Tunnel);
    }

    public ICommand SelectAllCommand { get; }
    public ICommand CutCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand PasteCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }

    protected override Type StyleKeyOverride { get; } = typeof(OvoTextEditor);

    private void UpdateCommandStates()
    {
        foreach (var command in _commands)
            command.RaiseCanExecuteChanged();
    }
}
