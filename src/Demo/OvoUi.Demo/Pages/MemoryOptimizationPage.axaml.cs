using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OvoUi.Platform;

namespace OvoUi.Demo.Pages;

public partial class MemoryOptimizationPage : UserControl
{
    public MemoryOptimizationPage()
    {
        InitializeComponent();
        DataContext = this;
    }

    public bool CanTrimWorkingSet => MemoryOptimization.IsWorkingSetTrimSupported;

    public string SourceCode { get; } = """
                                        using Avalonia.Interactivity;
                                        using OvoUi.Platform;

                                        // Set DataContext = this in the page constructor.
                                        public bool CanTrimWorkingSet => MemoryOptimization.IsWorkingSetTrimSupported;

                                        private void TrimWorkingSet(object? sender, RoutedEventArgs e)
                                        {
                                            StatusLabel.Text = MemoryOptimization.TryTrimWorkingSet(out var error)
                                                ? "Working set trim request completed."
                                                : error;
                                        }

                                        private void ForceGc(object? sender, RoutedEventArgs e)
                                        {
                                            MemoryOptimization.ForceGarbageCollection();
                                            StatusLabel.Text = "Forced GC completed.";
                                        }
                                        """;

    private void TrimWorkingSet(object? sender, RoutedEventArgs e) => RunOperation(trimWorkingSet: true);

    private void ForceGc(object? sender, RoutedEventArgs e) => RunOperation(trimWorkingSet: false);

    private void RunOperation(bool trimWorkingSet)
    {
        try
        {
            var before = ReadMemory();
            var started = Stopwatch.GetTimestamp();
            string? error = null;
            var success = true;
            if (trimWorkingSet)
                success = MemoryOptimization.TryTrimWorkingSet(out error);
            else
                MemoryOptimization.ForceGarbageCollection();
            var elapsed = Stopwatch.GetElapsedTime(started);
            var after = ReadMemory();

            StatusLabel.Text = success
                ? $"{(trimWorkingSet ? "Working set trim request" : "Forced GC")} completed in {elapsed.TotalMilliseconds:F2} ms."
                : $"Working set trim failed: {error}";
            MetricsLabel.Text =
                $"Working set: {FormatBytes(before.WorkingSet)} → {FormatBytes(after.WorkingSet)}\n" +
                $"Managed heap: {FormatBytes(before.ManagedHeap)} → {FormatBytes(after.ManagedHeap)}\n" +
                $"GC count delta: Gen 0 = {after.Gen0 - before.Gen0}, " +
                $"Gen 1 = {after.Gen1 - before.Gen1}, Gen 2 = {after.Gen2 - before.Gen2}";
        }
        catch (Exception exception)
        {
            StatusLabel.Text = $"Operation failed: {exception.Message}";
            MetricsLabel.Text = string.Empty;
        }
    }

    private static MemorySnapshot ReadMemory()
    {
        long? workingSet = null;
        // Browser runtimes do not expose process working set statistics.
        if (!OperatingSystem.IsBrowser())
        {
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            workingSet = process.WorkingSet64;
        }

        return new MemorySnapshot(workingSet, GC.GetTotalMemory(false),
            GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2));
    }

    private static string FormatBytes(long? bytes) =>
        bytes is { } value ? $"{value / 1024d / 1024d:F2} MiB" : "Unavailable on this platform";

    private readonly record struct MemorySnapshot(long? WorkingSet, long ManagedHeap, int Gen0, int Gen1, int Gen2);
}