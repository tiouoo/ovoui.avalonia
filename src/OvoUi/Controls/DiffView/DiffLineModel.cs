namespace OvoUi.Controls;

public class DiffLineModel
{
    public required string LeftText { get; init; }
    public required string RightText { get; init; }
    public required string LeftLineNumber { get; init; }
    public required string RightLineNumber { get; init; }
    public required DiffLineKind LeftKind { get; init; }
    public required DiffLineKind RightKind { get; init; }
    public bool LeftIsAdded => LeftKind == DiffLineKind.Added;
    public bool LeftIsRemoved => LeftKind == DiffLineKind.Removed;
    public bool LeftIsModified => LeftKind == DiffLineKind.Modified;
    public bool LeftIsEmpty => LeftKind == DiffLineKind.Empty;
    public bool RightIsAdded => RightKind == DiffLineKind.Added;
    public bool RightIsRemoved => RightKind == DiffLineKind.Removed;
    public bool RightIsModified => RightKind == DiffLineKind.Modified;
    public bool RightIsEmpty => RightKind == DiffLineKind.Empty;
}
