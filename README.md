# AppoMobi.Specials
![License](https://img.shields.io/github/license/taublast/AppoMobi.Specials.svg)
![NuGet Downloads](https://img.shields.io/nuget/dt/AppoMobi.Specials.svg)

## In-house .NET extensions and helpers

## ObservableRangeCollection&lt;T&gt;

An `ObservableCollection<T>` with batch operations that raise a **single truthful `CollectionChanged` event per operation** — carrying the real action, the affected items, and the exact starting index.

Most range-collection implementations degrade batch operations to `Reset` (a WPF `CollectionView` limitation made into a habit). `Reset` tells the consumer "throw everything away": a virtualizing list rebuilds its layout, re-measures items, and loses scroll position. Index-aware consumers — such as [DrawnUI](https://github.com/taublast/DrawnUi) `SkiaLayout` with recycled cells — can instead process an indexed `Add`/`Remove` incrementally: keep measurements, keep the viewport pinned, update only what changed. This collection guarantees the events they need.

### Operations

| Method | Event raised | Use for |
|---|---|---|
| `AddRange(items)` | one `Add`, items + tail index | append a batch (forward LoadMore) |
| `InsertRange(index, items)` | one `Add`, items + index | prepend/insert a block (backward LoadMore) |
| `RemoveRange(index, count)` | one `Remove`, items + index | drop a consecutive block (window trim) |
| `RemoveRange(items)` | one `Reset` | remove scattered items — they have no single index, so `Reset` is the only honest event |
| `ReplaceRange(items)` | one `Replace` | swap all contents in place, consumer keeps scroll position |
| `ReplaceRangeReset(items)` | one `Reset` | replace contents as a brand-new dataset, consumer rebuilds and resets scroll |
| `Replace(item)` | one `Replace` | replace all contents with a single item |

Every operation mutates `Items` silently first, then raises `Count` / `Item[]` property changes followed by the one collection event.

### Example: a sliding window over a large dataset

A list bound to millions of remote records never needs them all in memory. Keep a window of loaded items, extend it in the scroll direction, trim it at the opposite end:

```csharp
var items = new ObservableRangeCollection<Row>();

// forward load: append below
items.AddRange(nextBatch);

// backward load: prepend above — an index-aware consumer keeps the viewport pinned
items.InsertRange(0, previousBatch);

// memory cap: drop the far end as one indexed block
items.RemoveRange(0, overflow);                     // trim above
items.RemoveRange(items.Count - overflow, overflow); // trim below

// jump to an arbitrary position: rebase the window, consumer rebuilds at top
items.ReplaceRangeReset(batchAtTarget);
```

### Choosing between Replace and Reset

`ReplaceRange` and `ReplaceRangeReset` differ only in what they tell the consumer. `Replace` means "same list, new contents" — a virtualizing list swaps item contexts in place and stays where it is. `Reset` means "different list" — rebuild from scratch, scroll back to top. Pick by intent: refreshing visible data wants `ReplaceRange`, navigating to a new window of a dataset wants `ReplaceRangeReset`.


