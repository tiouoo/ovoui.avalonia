# 手动内存操作

安装 `OvoUi.Platform`，使用 `OvoUi.Platform.MemoryOptimization`：

```csharp
using OvoUi.Platform;

// 当前进程的 Windows 工作集裁剪；不支持或调用失败时返回 false，并提供原因。
if (!MemoryOptimization.TryTrimWorkingSet(out var error))
    Console.WriteLine(error);

// 阻塞式、积极的全代 GC，并压缩堆。
MemoryOptimization.ForceGarbageCollection();
```

Demo 导航：**Platform → MemoryOptimization**。两个按钮各自执行一次，展示工作集、托管堆的操作前后读数、各代 GC 次数增量和耗时。

| | 裁剪工作区（工作集） | 强制 GC |
| --- | --- | --- |
| 调用 | `K32EmptyWorkingSet(GetCurrentProcess())` | `GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true)` |
| 范围 | Windows 当前进程驻留的物理内存页面 | 当前进程的 .NET 托管堆 |
| 行为 | 请求移出可裁剪页面，不主动执行 GC | 回收符合条件的不可达对象并压缩堆，不主动裁剪工作集 |
| 结果 | 工作集可能下降，页面再次访问时会重新驻留 | 托管堆可能下降，工作集不保证下降 |
| 代价 | 后续访问可能产生更多缺页和延迟 | 阻塞式 GC 会暂停应用线程，界面可能卡顿 |
| 平台 | 仅 Windows，无需管理员权限 | 使用 .NET GC 的平台 |

两个 API 都不会释放仍被引用的托管对象，也不保证释放特定大小的内存。GC 方法不等待终结器，带终结器的对象可能需要后续回收才能释放。采样使用 `GC.GetTotalMemory(false)`，不因读取指标主动执行 GC；其他线程的分配和系统调度仍会影响结果。日常运行通常交由运行时和操作系统自动管理。

## 提取来源

- `E:\Portal\module\Ovo.Avalonia.Standard\src\Ovo.Avalonia.Standard\Modules\Platform\LoopGc.cs`：原代码每 10 秒调用 `GC.Collect(2, GCCollectionMode.Aggressive, true)`。这里改为单次调用，以 `GC.MaxGeneration` 表达全代回收，并显式启用堆压缩。
- `E:\Portal\src\Portal.Core\Services\SystemResources\MemoryOptimizationService.cs`：提取 `TrimCurrentProcessWorkingSet` 的 Windows 调用。未引入每 60 秒执行、工作集至少 120 MiB 的自动裁剪策略，按钮点击即可请求裁剪。
- Portal 的 `OptimizeWindowsMemory` 是另一套系统级内存操作，涉及提权，不属于这两个 API 的范围。
