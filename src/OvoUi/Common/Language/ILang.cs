namespace OvoUi.Common.Language;

public interface ILang
{
    public string Name { get; }
    public string FileName { get; }
    public string UpdateAt { get; }
    public string Type { get; }
    public string Size { get; }
    public string Cancel { get; }
    public string Close { get; }
    public string Confirm { get; }
    public string Ok { get; }
    public string ShowHiddenFiles { get; }
    public string FileAlreadyExists { get; }
    public string Day { get; }
    public string Month { get; }
    public string Year { get; }
    public string Hour { get; }
    public string Minute { get; }
    public string Second { get; }
    public string Undo { get; }
    public string Redo { get; }
    public string SelectAll { get; }
    public string Cut { get; }
    public string Copy { get; }
    public string Paste { get; }
    public string Clear { get; }
    public string JumpTo { get; }
    public string Page { get; }
    public string StartTime { get; }
    public string EndTime { get; }
    public string Yes { get; }
    public string No { get; }
    public string BringForward { get; }
    public string BringToFront { get; }
    public string SendBackward { get; }
    public string SendToBack { get; }
    public string Maximum { get; }
    public string Minimum { get; }
    public string Restore { get; }
    public string On { get; }
    public string Off { get; }
}