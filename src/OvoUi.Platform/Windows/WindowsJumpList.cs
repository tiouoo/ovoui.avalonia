using System.Runtime.InteropServices;

namespace OvoUi.Platform.Windows;

/// <summary>
/// A launch task displayed in the application's Windows taskbar jump list.
/// The category "Tasks" adds user tasks; other names create custom categories.
/// </summary>
public sealed record WindowsJumpListItem(
    string Category, string Title, string Description, string Arguments,
    string? IconPath = null, int IconIndex = 0);

/// <summary>Windows Jump Lists implemented with native COM calls, including Native AOT support.</summary>
public static unsafe partial class WindowsJumpList
{
    public static bool IsSupported => OperatingSystem.IsWindows();

    public static bool TrySetCurrentProcessAppId(string appId, out string? error) =>
        WindowsAppIdentity.TrySetCurrentProcessAppId(appId, out error);

    public static bool TryApply(string appId, IEnumerable<WindowsJumpListItem> items,
        out string? error, string? executablePath = null)
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
            var tasks = items.ToArray();
            foreach (var task in tasks)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(task.Category);
                ArgumentException.ThrowIfNullOrWhiteSpace(task.Title);
            }
            using var apartment = new ComApartment();
            using var destination = ComObject.Create(DestinationListClass, DestinationListInterface);
            SetString(destination, 3, appId); // ICustomDestinationList.SetAppID
            var arrayId = ObjectArrayInterface;
            uint slots;
            nint removed;
            Check(((delegate* unmanaged[Stdcall]<nint, uint*, Guid*, nint*, int>)destination.Table[4])(
                destination.Pointer, &slots, &arrayId, &removed));
            using var removedItems = new ComObject(removed);
            try
            {
                var path = executablePath ?? Environment.ProcessPath
                    ?? throw new InvalidOperationException("The current executable path is unavailable.");
                foreach (var category in tasks.GroupBy(task => task.Category))
                {
                    using var collection = ComObject.Create(ObjectCollectionClass, ObjectCollectionInterface);
                    foreach (var task in category)
                    {
                        using var link = CreateShellLink(task, path);
                        if (WasRemoved(removedItems, task.Arguments, path)) continue;
                        Check(((delegate* unmanaged[Stdcall]<nint, nint, int>)collection.Table[5])(
                            collection.Pointer, link.Pointer)); // IObjectCollection.AddObject
                    }
                    if (category.Key == "Tasks")
                        Check(((delegate* unmanaged[Stdcall]<nint, nint, int>)destination.Table[7])(
                            destination.Pointer, collection.Pointer)); // AddUserTasks
                    else
                        fixed (char* name = category.Key)
                            Check(((delegate* unmanaged[Stdcall]<nint, char*, nint, int>)destination.Table[5])(
                                destination.Pointer, name, collection.Pointer)); // AppendCategory
                }
                Call(destination, 8); // CommitList
            }
            catch
            {
                // Preserve the original failure if AbortList also fails.
                ((delegate* unmanaged[Stdcall]<nint, int>)destination.Table[11])(destination.Pointer);
                throw;
            }
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
            using var apartment = new ComApartment();
            using var destination = ComObject.Create(DestinationListClass, DestinationListInterface);
            SetString(destination, 10, appId); // DeleteList
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static ComObject CreateShellLink(WindowsJumpListItem item, string executablePath)
    {
        var link = ComObject.Create(ShellLinkClass, ShellLinkInterface);
        try
        {
            SetString(link, 20, executablePath); // IShellLinkW.SetPath
            SetString(link, 11, item.Arguments);
            SetString(link, 7, item.Description);
            fixed (char* icon = item.IconPath ?? executablePath)
                Check(((delegate* unmanaged[Stdcall]<nint, char*, int, int>)link.Table[17])(
                    link.Pointer, icon, item.IconIndex));
            using var properties = link.Query(PropertyStoreInterface);
            var key = new PropertyKey { FormatId = new("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), Id = 2 };
            var title = new PropVariant { Type = 31, Pointer = Marshal.StringToCoTaskMemUni(item.Title) };
            try
            {
                Check(((delegate* unmanaged[Stdcall]<nint, PropertyKey*, PropVariant*, int>)properties.Table[6])(
                    properties.Pointer, &key, &title));
                Call(properties, 7); // IPropertyStore.Commit
            }
            finally { PropVariantClear(ref title); }
            return link;
        }
        catch { link.Dispose(); throw; }
    }

    private static bool WasRemoved(ComObject removed, string arguments, string path)
    {
        uint count;
        Check(((delegate* unmanaged[Stdcall]<nint, uint*, int>)removed.Table[3])(removed.Pointer, &count));
        var id = ShellLinkInterface;
        // Shell links expose at most INFOTIPSIZE characters for arguments and MAX_PATH for GetPath.
        var pathBuffer = new char[32768];
        var argumentBuffer = new char[32768];
        for (uint index = 0; index < count; index++)
        {
            nint pointer;
            var hr = ((delegate* unmanaged[Stdcall]<nint, uint, Guid*, nint*, int>)removed.Table[4])(
                removed.Pointer, index, &id, &pointer);
            if (hr == unchecked((int)0x80004002)) continue; // A removed entry need not be a shell link.
            Check(hr);
            using var link = new ComObject(pointer);
            fixed (char* file = pathBuffer)
            fixed (char* args = argumentBuffer)
            {
                Check(((delegate* unmanaged[Stdcall]<nint, char*, int, nint, uint, int>)link.Table[3])(
                    link.Pointer, file, pathBuffer.Length, 0, 4)); // SLGP_RAWPATH
                Check(((delegate* unmanaged[Stdcall]<nint, char*, int, int>)link.Table[10])(
                    link.Pointer, args, argumentBuffer.Length));
                if (string.Equals(new string(file), path, StringComparison.OrdinalIgnoreCase) &&
                    new string(args) == arguments) return true;
            }
        }
        return false;
    }

    private static void SetString(ComObject target, int slot, string value)
    {
        fixed (char* text = value)
            Check(((delegate* unmanaged[Stdcall]<nint, char*, int>)target.Table[slot])(target.Pointer, text));
    }

    private static void Call(ComObject target, int slot) =>
        Check(((delegate* unmanaged[Stdcall]<nint, int>)target.Table[slot])(target.Pointer));

    private static void Check(int result) => Marshal.ThrowExceptionForHR(result);

    // Own native interface pointers explicitly; no runtime COM wrappers or dynamic code are used.
    private sealed class ComObject(nint pointer) : IDisposable
    {
        public nint Pointer { get; private set; } = pointer;
        public nint* Table => *(nint**)Pointer;
        public static ComObject Create(Guid classId, Guid interfaceId)
        {
            Check(CoCreateInstance(in classId, 0, 1, in interfaceId, out var result));
            return new ComObject(result);
        }
        public ComObject Query(Guid id)
        {
            nint result;
            Check(((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)Table[0])(Pointer, &id, &result));
            return new ComObject(result);
        }
        public void Dispose()
        {
            if (Pointer == 0) return;
            ((delegate* unmanaged[Stdcall]<nint, uint>)Table[2])(Pointer);
            Pointer = 0;
        }
    }

    private sealed class ComApartment : IDisposable
    {
        private readonly bool _uninitialize;
        public ComApartment()
        {
            var result = CoInitializeEx(0, 2); // COINIT_APARTMENTTHREADED
            // An already initialized MTA can also call these shell interfaces.
            if (result != unchecked((int)0x80010106)) Check(result);
            _uninitialize = result >= 0;
        }
        public void Dispose() { if (_uninitialize) CoUninitialize(); }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct PropertyKey { public Guid FormatId; public uint Id; }

    // PROPVARIANT is 24 bytes on x64 and 16 bytes on x86 (the union includes a counted array).
    [StructLayout(LayoutKind.Sequential)]
    private struct PropVariant
    {
        public ushort Type;
        private ushort _reserved1, _reserved2, _reserved3;
        public nint Pointer;
        private nint _unionPadding;
    }

    private static readonly Guid DestinationListClass = new("77F10CF0-3DB5-4966-B520-B7C54FD35ED6");
    private static readonly Guid DestinationListInterface = new("6332DEBF-87B5-4670-90C0-5E57B408A49E");
    private static readonly Guid ObjectCollectionClass = new("2D3468C1-36A7-43B6-AC24-D3F02FD9607A");
    private static readonly Guid ObjectCollectionInterface = new("5632B1A4-E38A-400A-928A-D4CD63230295");
    private static readonly Guid ObjectArrayInterface = new("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9");
    private static readonly Guid ShellLinkClass = new("00021401-0000-0000-C000-000000000046");
    private static readonly Guid ShellLinkInterface = new("000214F9-0000-0000-C000-000000000046");
    private static readonly Guid PropertyStoreInterface = new("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99");

    [LibraryImport("ole32.dll")]
    private static partial int CoCreateInstance(in Guid classId, nint outer, uint context, in Guid interfaceId, out nint result);
    [LibraryImport("ole32.dll")]
    private static partial int CoInitializeEx(nint reserved, uint flags);
    [LibraryImport("ole32.dll")]
    private static partial void CoUninitialize();
    [LibraryImport("ole32.dll")]
    private static partial int PropVariantClear(ref PropVariant variant);
}
