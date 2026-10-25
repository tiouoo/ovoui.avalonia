using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace OvoUi.Platform;

/// <summary>Explicit, one-shot memory operations for the current process.</summary>
public static class MemoryOptimization
{
    /// <summary>Whether the operating system supports trimming the current process working set.</summary>
    public static bool IsWorkingSetTrimSupported => OperatingSystem.IsWindows();

    /// <summary>Immediately asks Windows to trim resident pages from the current process working set.</summary>
    /// <remarks>
    /// Does not run the GC or free live objects. Pages can become resident again when accessed.
    /// No administrator privileges are required. No timer or minimum working set threshold is used.
    /// </remarks>
    /// <param name="error">An explanation if unsupported or if the native call fails; otherwise null.</param>
    /// <returns>Whether Windows accepted the trim request, not whether memory usage decreased.</returns>
    public static bool TryTrimWorkingSet(out string? error)
    {
        error = null;
        if (!IsWorkingSetTrimSupported)
        {
            error = "Working set trimming is only available on Windows.";
            return false;
        }

        if (K32EmptyWorkingSet(GetCurrentProcess()))
            return true;

        error = new Win32Exception(Marshal.GetLastWin32Error()).Message;
        return false;
    }

    /// <summary>Immediately performs a blocking, aggressive full collection with heap compaction.</summary>
    /// <remarks>
    /// Reclaims eligible unreachable managed objects, and can pause application threads.
    /// Does not explicitly trim the Windows working set or wait for pending finalizers.
    /// Neither a smaller working set nor a particular number of reclaimed bytes is guaranteed.
    /// Prefer normal runtime collection for routine application use.
    /// </remarks>
    public static void ForceGarbageCollection() =>
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool K32EmptyWorkingSet(IntPtr process);
}