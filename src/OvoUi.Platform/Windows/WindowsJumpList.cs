using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace OvoUi.Platform.Windows;

/// <summary>
/// Describes a launch task displayed in a Windows taskbar jump list.
/// </summary>
public sealed record WindowsJumpListItem(
    string Category,
    string Title,
    string Description,
    string Arguments,
    string? IconPath = null,
    int IconIndex = 0);

/// <summary>
/// Creates the jump list shown when the user right-clicks an application's Windows taskbar icon.
/// </summary>
public static partial class WindowsJumpList
{
    public static bool IsSupported => OperatingSystem.IsWindows();

    public static bool TrySetCurrentProcessAppId(string appId, out string? error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appId);
        if (!IsSupported)
        {
            error = "Windows jump lists are only available on Windows.";
            return false;
        }

        try
        {
            SetCurrentProcessExplicitAppUserModelID(appId);
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    public static bool TryApply(
        string appId,
        IEnumerable<WindowsJumpListItem> items,
        out string? error,
        string? executablePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appId);
        ArgumentNullException.ThrowIfNull(items);
        if (!IsSupported)
        {
            error = "Windows jump lists are only available on Windows.";
            return false;
        }

        try
        {
            Apply(appId, items, executablePath ?? Environment.ProcessPath
                ?? throw new InvalidOperationException("The current executable path is unavailable."));
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    public static bool TryClear(string appId, out string? error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appId);
        if (!IsSupported)
        {
            error = "Windows jump lists are only available on Windows.";
            return false;
        }

        try
        {
#pragma warning disable SYSLIB1099
            ((ICustomDestinationList)new DestinationList()).DeleteList(appId);
#pragma warning restore SYSLIB1099
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static void Apply(string appId, IEnumerable<WindowsJumpListItem> items, string executablePath)
    {
        var materializedItems = items.ToArray();
        foreach (var item in materializedItems)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(item.Category);
            ArgumentException.ThrowIfNullOrWhiteSpace(item.Title);
        }

#pragma warning disable SYSLIB1099
        var destinationList = (ICustomDestinationList)new DestinationList();
#pragma warning restore SYSLIB1099
        destinationList.SetAppId(appId);
        var objectArrayGuid = typeof(IObjectArray).GUID;
        destinationList.BeginList(out _, ref objectArrayGuid, out _);
        try
        {
            foreach (var category in materializedItems.GroupBy(item => item.Category))
            {
                var collection = new ObjectCollection();
                foreach (var item in category)
                    collection.AddObject(CreateShellLink(item, executablePath));
                destinationList.AppendCategory(category.Key, collection);
            }

            destinationList.CommitList();
        }
        catch
        {
            destinationList.AbortList();
            throw;
        }
    }

    private static IShellLinkW CreateShellLink(WindowsJumpListItem item, string executablePath)
    {
        var link = (IShellLinkW)new ShellLink();
        link.SetPath(executablePath);
        link.SetArguments(item.Arguments);
        link.SetDescription(item.Description);
        link.SetIconLocation(item.IconPath ?? executablePath, item.IconIndex);

        var propertyStore = (IPropertyStore)link;
        var key = PropertyKeys.Title;
        var title = new PropVariant(item.Title);
        try
        {
            propertyStore.SetValue(ref key, ref title);
            propertyStore.Commit();
        }
        finally
        {
            PropVariantClear(ref title);
        }

        return link;
    }

    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial void SetCurrentProcessExplicitAppUserModelID(string appId);

    [LibraryImport("ole32.dll")]
    private static partial void PropVariantClear(ref PropVariant variant);

    [GeneratedComInterface]
    [Guid("6332DEBF-87B5-4670-90C0-5E57B408A49E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal partial interface ICustomDestinationList
    {
        void SetAppId([MarshalAs(UnmanagedType.LPWStr)] string appId);
        void BeginList(out uint maxSlots, ref Guid interfaceId, [MarshalAs(UnmanagedType.Interface)] out object removedItems);
        void AppendCategory([MarshalAs(UnmanagedType.LPWStr)] string category, IObjectArray objects);
        void AppendKnownCategory(int category);
        void AddUserTasks(IObjectArray tasks);
        void CommitList();
        void GetRemovedDestinations(ref Guid interfaceId, [MarshalAs(UnmanagedType.Interface)] out object removedItems);
        void DeleteList([MarshalAs(UnmanagedType.LPWStr)] string appId);
        void AbortList();
    }

    [GeneratedComInterface]
    [Guid("5632B1A4-E38A-400A-928A-D4CD63230295")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal partial interface IObjectCollection : IObjectArray
    {
        void AddObject([MarshalAs(UnmanagedType.Interface)] object item);
        void AddFromArray(IObjectArray source);
        void RemoveObjectAt(uint index);
        void Clear();
    }

    [GeneratedComInterface]
    [Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal partial interface IObjectArray
    {
        void GetCount(out uint count);
        void GetAt(uint index, ref Guid interfaceId, [MarshalAs(UnmanagedType.Interface)] out object item);
    }

    [GeneratedComClass]
    internal sealed partial class ObjectCollection : IObjectCollection
    {
        private readonly List<object> _items = [];

        public void GetCount(out uint count) => count = (uint)_items.Count;

        public void GetAt(uint index, ref Guid interfaceId, out object item) => item = _items[(int)index];

        public void AddObject(object item) => _items.Add(item);

        public void AddFromArray(IObjectArray source)
        {
            source.GetCount(out var count);
            for (uint index = 0; index < count; index++)
            {
                var objectGuid = typeof(object).GUID;
                source.GetAt(index, ref objectGuid, out var item);
                _items.Add(item);
            }
        }

        public void RemoveObjectAt(uint index) => _items.RemoveAt((int)index);

        public void Clear() => _items.Clear();
    }

    [ComImport]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder file, int capacity, out IntPtr findData, uint flags);
        void GetIDList(out IntPtr itemIdList);
        void SetIDList(IntPtr itemIdList);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder name, int capacity);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string name);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder directory, int capacity);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string directory);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder arguments, int capacity);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string arguments);
        void GetHotkey(out short hotkey);
        void SetHotkey(short hotkey);
        void GetShowCmd(out int showCommand);
        void SetShowCmd(int showCommand);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder iconPath, int capacity, out int iconIndex);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string iconPath, int iconIndex);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string path, uint reserved);
        void Resolve(IntPtr windowHandle, uint flags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string file);
    }

    [ComImport]
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPropertyStore
    {
        void GetCount(out uint count);
        void GetAt(uint index, out PropertyKey key);
        void GetValue(ref PropertyKey key, out PropVariant value);
        void SetValue(ref PropertyKey key, ref PropVariant value);
        void Commit();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct PropertyKey(Guid formatId, uint propertyId)
    {
        public Guid FormatId = formatId;
        public uint PropertyId = propertyId;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct PropVariant
    {
        [FieldOffset(0)] private ushort _valueType;
        [FieldOffset(8)] private IntPtr _pointerValue;

        public PropVariant(string value)
        {
            this = default;
            _valueType = 31;
            _pointerValue = Marshal.StringToCoTaskMemUni(value);
        }
    }

    private static class PropertyKeys
    {
        public static readonly PropertyKey Title = new(new Guid("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), 2);
    }

    [ComImport]
    [Guid("77F10CF0-3DB5-4966-B520-B7C54FD35ED6")]
    private class DestinationList;

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    private class ShellLink;
}
