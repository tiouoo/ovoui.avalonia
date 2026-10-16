namespace OvoUi.Tab;

/// <summary>
/// User-visible labels used by the built-in tab UI. Replace this object to localize a host.
/// </summary>
public sealed class OvoTabStrings
{
    public static OvoTabStrings English { get; } = new();

    public static OvoTabStrings SimplifiedChinese { get; } = new()
    {
        NewTab = "新建标签页",
        CloseTab = "关闭标签页",
        CloseOtherTabs = "关闭其他标签页",
        MoveToWindow = "转移到窗口",
        OpenInNewWindow = "在新窗口打开",
        ReorderTabs = "排序标签页",
        MoveBackward = "向前移动",
        MoveForward = "向后移动",
        MoveToStart = "移动到最前",
        MoveToEnd = "移动到最后"
    };

    public string NewTab { get; init; } = "New tab";
    public string CloseTab { get; init; } = "Close tab";
    public string CloseOtherTabs { get; init; } = "Close other tabs";
    public string MoveToWindow { get; init; } = "Move to window";
    public string OpenInNewWindow { get; init; } = "Open in new window";
    public string ReorderTabs { get; init; } = "Reorder tabs";
    public string MoveBackward { get; init; } = "Move backward";
    public string MoveForward { get; init; } = "Move forward";
    public string MoveToStart { get; init; } = "Move to start";
    public string MoveToEnd { get; init; } = "Move to end";
}
