using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Controls;

namespace OvoUi.Platform.Windows;

/// <summary>
/// Adds application commands to the native system menu shown from a Windows taskbar icon or title bar.
/// </summary>
public static class WindowsSystemMenu
{
    private const uint WmNcDestroy = 0x0082;
    private const uint WmSysCommand = 0x0112;
    private const uint ScClose = 0xF060;
    private const uint MfByCommand = 0x0000;
    private const uint MfString = 0x0000;
    private const uint MiimId = 0x0002;
    private const uint MiimFtype = 0x0100;
    private const uint MftSeparator = 0x0800;
    private const uint FirstCommandId = 0xA000;
    private const uint LastCommandId = 0xEFF0;
    private const uint CommandIdStep = 0x10;
    private const uint TpmRightButton = 0x0002;
    private const uint TpmReturnCommand = 0x0100;
    private const nuint SubclassId = 0x4F564F55;

    private static readonly object SyncRoot = new();
    private static readonly Dictionary<IntPtr, WindowMenuState> States = [];
    private static readonly SubclassProc WindowSubclassProc = HandleWindowMessage;
    private static uint _nextCommandId = FirstCommandId;

    public static bool IsSupported => OperatingSystem.IsWindows();

    /// <summary>
    /// Adds a command immediately above Close. This method is safe to call on every platform.
    /// </summary>
    public static bool TryAddItem(
        Window window,
        string text,
        Action clicked,
        out WindowsSystemMenuItem? item,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentNullException.ThrowIfNull(clicked);
        item = null;

        if (!TryGetWindowHandle(window, out var windowHandle, out error))
            return false;

        try
        {
            item = AddItem(windowHandle, text, clicked);
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    /// <summary>
    /// Adds a separator immediately above Close. This method is safe to call on every platform.
    /// </summary>
    public static bool TryAddSeparator(
        Window window,
        out WindowsSystemMenuItem? item,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(window);
        item = null;

        if (!TryGetWindowHandle(window, out var windowHandle, out error))
            return false;

        try
        {
            item = AddSeparator(windowHandle);
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    /// <summary>
    /// Removes every item added to a window through this helper.
    /// </summary>
    public static bool TryClear(Window window, out string? error)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (!TryGetWindowHandle(window, out var windowHandle, out error))
            return false;

        try
        {
            Clear(windowHandle);
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    /// <summary>
    /// Shows the system menu at the current pointer position. This method is safe to call on every platform.
    /// </summary>
    public static bool TryShow(Window window, out string? error)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (!TryGetWindowHandle(window, out var windowHandle, out error))
            return false;

        try
        {
            Show(windowHandle);
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    [SupportedOSPlatform("windows")]
    public static WindowsSystemMenuItem AddItem(IntPtr windowHandle, string text, Action clicked)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentNullException.ThrowIfNull(clicked);

        lock (SyncRoot)
        {
            var state = GetOrCreateState(windowHandle);
            var commandId = GetNextCommandId();
            if (!InsertMenu(state.MenuHandle, ScClose, MfByCommand | MfString, commandId, text))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            state.Commands.Add(commandId, clicked);
            DrawMenuBar(windowHandle);
            return new WindowsSystemMenuItem(windowHandle, commandId);
        }
    }

    [SupportedOSPlatform("windows")]
    public static WindowsSystemMenuItem AddSeparator(IntPtr windowHandle)
    {
        lock (SyncRoot)
        {
            var state = GetOrCreateState(windowHandle);
            var commandId = GetNextCommandId();
            var itemInfo = new MenuItemInfo
            {
                Size = (uint)Marshal.SizeOf<MenuItemInfo>(),
                Mask = MiimId | MiimFtype,
                Type = MftSeparator,
                Id = commandId
            };
            if (!InsertMenuItem(state.MenuHandle, ScClose, false, ref itemInfo))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            state.Commands.Add(commandId, null);
            DrawMenuBar(windowHandle);
            return new WindowsSystemMenuItem(windowHandle, commandId);
        }
    }

    [SupportedOSPlatform("windows")]
    public static bool RemoveItem(IntPtr windowHandle, uint commandId)
    {
        lock (SyncRoot)
        {
            if (!States.TryGetValue(windowHandle, out var state) || !state.Commands.Remove(commandId))
                return false;

            var removed = RemoveMenu(state.MenuHandle, commandId, MfByCommand);
            DrawMenuBar(windowHandle);
            RemoveStateWhenEmpty(windowHandle, state);
            return removed;
        }
    }

    [SupportedOSPlatform("windows")]
    public static void Clear(IntPtr windowHandle)
    {
        lock (SyncRoot)
        {
            if (!States.Remove(windowHandle, out var state))
                return;

            foreach (var commandId in state.Commands.Keys)
                RemoveMenu(state.MenuHandle, commandId, MfByCommand);

            RemoveWindowSubclass(windowHandle, WindowSubclassProc, SubclassId);
            DrawMenuBar(windowHandle);
        }
    }

    [SupportedOSPlatform("windows")]
    public static void Show(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
            throw new ArgumentException("A valid native window handle is required.", nameof(windowHandle));

        var menuHandle = GetSystemMenu(windowHandle, false);
        if (menuHandle == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        if (!GetCursorPos(out var position))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        SetForegroundWindow(windowHandle);
        var commandId = TrackPopupMenu(
            menuHandle,
            TpmRightButton | TpmReturnCommand,
            position.X,
            position.Y,
            0,
            windowHandle,
            IntPtr.Zero);
        if (commandId != 0)
            SendMessage(windowHandle, WmSysCommand, new IntPtr(commandId), IntPtr.Zero);
    }

    private static bool TryGetWindowHandle(Window window, out IntPtr windowHandle, out string? error)
    {
        windowHandle = IntPtr.Zero;
        if (!IsSupported)
        {
            error = "Windows system menus are only available on Windows.";
            return false;
        }

        windowHandle = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
        if (windowHandle != IntPtr.Zero)
        {
            error = null;
            return true;
        }

        error = "The window does not have a native handle yet. Add menu items after the window is opened.";
        return false;
    }

    [SupportedOSPlatform("windows")]
    private static WindowMenuState GetOrCreateState(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
            throw new ArgumentException("A valid native window handle is required.", nameof(windowHandle));
        if (States.TryGetValue(windowHandle, out var state))
            return state;

        var menuHandle = GetSystemMenu(windowHandle, false);
        if (menuHandle == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        if (!SetWindowSubclass(windowHandle, WindowSubclassProc, SubclassId, IntPtr.Zero))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        state = new WindowMenuState(menuHandle);
        States.Add(windowHandle, state);
        return state;
    }

    [SupportedOSPlatform("windows")]
    private static void RemoveStateWhenEmpty(IntPtr windowHandle, WindowMenuState state)
    {
        if (state.Commands.Count != 0)
            return;
        States.Remove(windowHandle);
        RemoveWindowSubclass(windowHandle, WindowSubclassProc, SubclassId);
    }

    private static uint GetNextCommandId()
    {
        for (var attempts = 0u; attempts <= LastCommandId - FirstCommandId; attempts++)
        {
            var candidate = _nextCommandId;
            _nextCommandId = candidate == LastCommandId ? FirstCommandId : candidate + CommandIdStep;
            if (States.Values.All(state => !state.Commands.ContainsKey(candidate)))
                return candidate;
        }

        throw new InvalidOperationException("No Windows system-menu command IDs are available.");
    }

    private static IntPtr HandleWindowMessage(
        IntPtr windowHandle,
        uint message,
        IntPtr wParam,
        IntPtr lParam,
        nuint subclassId,
        IntPtr referenceData)
    {
        if (message == WmSysCommand)
        {
            Action? clicked = null;
            lock (SyncRoot)
            {
                if (States.TryGetValue(windowHandle, out var state))
                    state.Commands.TryGetValue(unchecked((uint)wParam.ToInt64()) & 0xFFF0, out clicked);
            }

            if (clicked is not null)
            {
                try
                {
                    clicked();
                }
                catch (Exception exception)
                {
                    Trace.TraceError($"Windows system-menu callback failed: {exception}");
                }
                return IntPtr.Zero;
            }
        }

        if (message == WmNcDestroy)
        {
            lock (SyncRoot)
                States.Remove(windowHandle);
        }

        return DefSubclassProc(windowHandle, message, wParam, lParam);
    }

    private sealed class WindowMenuState(IntPtr menuHandle)
    {
        public IntPtr MenuHandle { get; } = menuHandle;
        public Dictionary<uint, Action?> Commands { get; } = [];
    }

    private delegate IntPtr SubclassProc(
        IntPtr windowHandle,
        uint message,
        IntPtr wParam,
        IntPtr lParam,
        nuint subclassId,
        IntPtr referenceData);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MenuItemInfo
    {
        public uint Size;
        public uint Mask;
        public uint Type;
        public uint State;
        public uint Id;
        public IntPtr SubMenu;
        public IntPtr CheckedBitmap;
        public IntPtr UncheckedBitmap;
        public nuint ItemData;
        public IntPtr TypeData;
        public uint TextLength;
        public IntPtr ItemBitmap;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetSystemMenu(IntPtr windowHandle, [MarshalAs(UnmanagedType.Bool)] bool revert);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool InsertMenu(IntPtr menuHandle, uint position, uint flags, nuint itemId, string text);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool InsertMenuItem(
        IntPtr menuHandle,
        uint item,
        [MarshalAs(UnmanagedType.Bool)] bool byPosition,
        ref MenuItemInfo menuItemInfo);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveMenu(IntPtr menuHandle, uint position, uint flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DrawMenuBar(IntPtr windowHandle);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out NativePoint point);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr windowHandle);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int TrackPopupMenu(
        IntPtr menuHandle,
        uint flags,
        int x,
        int y,
        int reserved,
        IntPtr windowHandle,
        IntPtr rectangle);

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern IntPtr SendMessage(
        IntPtr windowHandle,
        uint message,
        IntPtr wParam,
        IntPtr lParam);

    [DllImport("comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowSubclass(
        IntPtr windowHandle,
        SubclassProc subclassProc,
        nuint subclassId,
        IntPtr referenceData);

    [DllImport("comctl32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveWindowSubclass(
        IntPtr windowHandle,
        SubclassProc subclassProc,
        nuint subclassId);

    [DllImport("comctl32.dll")]
    private static extern IntPtr DefSubclassProc(IntPtr windowHandle, uint message, IntPtr wParam, IntPtr lParam);
}

/// <summary>
/// Represents one item registered in a Windows system menu.
/// </summary>
public sealed class WindowsSystemMenuItem : IDisposable
{
    private readonly IntPtr _windowHandle;
    private bool _removed;

    internal WindowsSystemMenuItem(IntPtr windowHandle, uint commandId)
    {
        _windowHandle = windowHandle;
        CommandId = commandId;
    }

    public uint CommandId { get; }

    public bool Remove()
    {
        if (_removed)
            return false;
        _removed = true;
        return WindowsSystemMenu.RemoveItem(_windowHandle, CommandId);
    }

    public void Dispose() => Remove();
}
