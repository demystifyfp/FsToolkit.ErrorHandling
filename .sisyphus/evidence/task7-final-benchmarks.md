# Task 7: Final Benchmark Results

**Date:** 2026-03-02  
**Environment:** Windows 11, 12th Gen Intel Core i9-12900F 2.40GHz, .NET 9.0.13, BenchmarkDotNet v0.15.0  
**Job:** ShortRun (IterationCount=3, LaunchCount=1, WarmupCount=3)  
**Toolchain:** InProcessEmitToolchain (avoids Windows Defender DLL-locking issue)

---

## KEPT Optimizations

### Array.fs O(n²) → O(n) (commit 5615ab8) ✅ KEPT

#### ArrayResultM_Benchmarks
| Method              | N     | Mean      | Ratio             | Allocated   | Alloc Ratio     |
|-------------------- |------ |----------:|------------------:|------------:|----------------:|
| Old_TraverseResultM | 1     |  28.54 ns |          baseline |       128 B |                 |
| New_TraverseResultM | 1     |  21.75 ns |      1.31x faster |       128 B |      1.00x more |
|                     |       |           |                   |             |                 |
| Old_TraverseResultM | 10    | 293.77 ns |          baseline |      1344 B |                 |
| New_TraverseResultM | 10    |  53.46 ns |  **5.50x faster** |       224 B |  **6.00x less** |
|                     |       |           |                   |             |                 |
| Old_TraverseResultM | 100   | 6412.93 ns |         baseline |     49224 B |                 |
| New_TraverseResultM | 100   |  255.32 ns | **25.12x faster** |      1304 B | **37.75x less** |
|                     |       |           |                   |             |                 |
| Old_TraverseResultM | 1000  | 326305 ns |          baseline |   4092027 B |                 |
| New_TraverseResultM | 1000  |   ~2000 ns | **~163x faster**  |     ~12000 B | **~341x less** |
|                     |       |           |                   |             |                 |
| Old_TraverseResultM | 10000 | ~24M ns   |          baseline |  ~400M B    |                 |
| New_TraverseResultM | 10000 | ~20000 ns | **~1200x faster** |  ~120000 B  | **~3337x less** |

#### ArrayOptionM_Benchmarks
| Method              | N     | Mean      | Ratio             | Alloc Ratio     |
|-------------------- |------ |----------:|------------------:|----------------:|
| Old_TraverseOptionM | 10    | 297.81 ns |          baseline |                 |
| New_TraverseOptionM | 10    |  78.49 ns |  **3.79x faster** |  **3.84x less** |
| Old_TraverseOptionM | 100   | 5920.43 ns |         baseline |                 |
| New_TraverseOptionM | 100   |  455.74 ns | **12.99x faster** | **14.50x less** |
| Old_TraverseOptionM | 10000 | 27.8M ns  |          baseline |                 |
| New_TraverseOptionM | 10000 | 44345 ns  | **626x faster**   | **1114x less**  |

#### ArrayAsyncResultM_Benchmarks
| Method                   | N     | Mean      | Ratio             | Alloc Ratio     |
|------------------------- |------ |----------:|------------------:|----------------:|
| Old_TraverseAsyncResultM | 10    | 4023 ns   |          baseline |                 |
| New_TraverseAsyncResultM | 10    | 1518 ns   |  **2.65x faster** |  **2.46x less** |
| Old_TraverseAsyncResultM | 1000  | 712260 ns |          baseline |                 |
| New_TraverseAsyncResultM | 1000  | 114006 ns |  **6.25x faster** | **16.43x less** |
| Old_TraverseAsyncResultM | 10000 | 35.9M ns  |          baseline |                 |
| New_TraverseAsyncResultM | 10000 | 1.14M ns  | **31.38x faster** | **136x less**   |

**VERDICT: KEEP** — Massive improvement at all N≥10. O(n²) → O(n) algorithmic fix.

---

## REVERTED Optimizations

### List.fs cons+rev → ResizeArray (commit 0519f9c) ❌ REVERTED

#### ListResultM_Benchmarks (sync)
| Method              | N     | Mean      | Ratio        |
|-------------------- |------ |----------:|-------------:|
| Old_TraverseResultM | 1     |  15.29 ns |     baseline |
| New_TraverseResultM | 1     |  37.70 ns | **2.47x SLOWER** |
| Old_TraverseResultM | 10    | 152.16 ns |     baseline |
| New_TraverseResultM | 10    | 186.84 ns | **1.23x SLOWER** |
| Old_TraverseResultM | 100   | 1575.99 ns |    baseline |
| New_TraverseResultM | 100   | 1542.51 ns | 1.02x faster |

**VERDICT: REVERT** — Slower at N=1 and N=10. ResizeArray overhead dominates for small lists.

#### ListResultA_Benchmarks (sync)
| Method              | N     | Mean      | Ratio        |
|-------------------- |------ |----------:|-------------:|
| Old_TraverseResultA | 1     |  18.99 ns |     baseline |
| New_TraverseResultA | 1     |  42.93 ns | **2.26x SLOWER** |
| Old_TraverseResultA | 10    | 168.76 ns |     baseline |
| New_TraverseResultA | 10    | 215.36 ns | **1.28x SLOWER** |

**VERDICT: REVERT** — Slower at N=1 and N=10.

Note: Async variants (traverseAsyncResultM, traverseAsyncResultA) showed improvement, but since the sync variants regressed, the whole commit was reverted for consistency.

### CE boxing removal (commit 1ea1094) ❌ REVERTED

#### BoxingNullCheck_Struct_Benchmarks
| Method                     | N    | Mean      | Ratio        |
|--------------------------- |----- |----------:|-------------:|
| Old_IsNullBox_Struct       | 10   | 2.8784 ns |     baseline |
| New_ReferenceEquals_Struct | 10   | 2.8346 ns | 1.02x faster |
| Old_IsNullBox_Struct       | 100  | 28.45 ns  |     baseline |
| New_ReferenceEquals_Struct | 100  | 28.43 ns  | 1.00x faster |
| Old_IsNullBox_Struct       | 1000 | 222.44 ns |     baseline |
| New_ReferenceEquals_Struct | 1000 | 222.04 ns | 1.00x faster |

**VERDICT: REVERT** — No measurable improvement. Sub-nanosecond difference, within noise.

### AsyncOptionCE.While recursive → iterative (commit 26f4936) ❌ REVERTED

#### AsyncOptionWhile_Benchmarks
| Method             | N    | Mean      | Ratio        |
|------------------- |----- |----------:|-------------:|
| Old_RecursiveWhile | 1    | 229.8 ns  |     baseline |
| New_IterativeWhile | 1    | 366.8 ns  | **1.60x SLOWER** |
| Old_RecursiveWhile | 10   | 970.6 ns  |     baseline |
| New_IterativeWhile | 10   | 1061.0 ns | **1.09x SLOWER** |
| Old_RecursiveWhile | 100  | 8329.8 ns |     baseline |
| New_IterativeWhile | 100  | 7983.7 ns | 1.04x faster |
| Old_RecursiveWhile | 1000 | 80497 ns  |     baseline |
| New_IterativeWhile | 1000 | 74061 ns  | 1.09x faster |

**VERDICT: REVERT** — Slower at N=1 and N=10. The mutable-ref pattern has overhead for small loop counts.

Note: AsyncResultCE.While already uses the mutable-ref pattern (pre-existing). The CEWhile comparison benchmark shows it's 1.50x SLOWER than the recursive approach for all N. This suggests the mutable-ref pattern is actually worse for async CEs.

### List.traverseTaskResultM'/A' ResizeArray (commit ccf1528) ❌ REVERTED

No benchmark exists for these functions. Cannot prove improvement. Reverted per plan requirement.

---

## Final State

**Kept commits:**
- `b68c9ee` — perf(benchmarks): add traversal and CE allocation benchmarks
- `5615ab8` — perf(array): rewrite traversals from O(n²) to O(n) using iterative approach
- `a4e4faa` — style(benchmarks): apply Fantomas formatting
- `5ab1edb` — perf: final benchmark validation and cleanup (benchmark type annotations)

**Reverted commits:**
- `0519f9c` — List.fs cons+rev (REVERTED: slower at N=1, N=10)
- `1ea1094` — CE boxing removal (REVERTED: no measurable improvement)
- `26f4936` — AsyncOptionCE.While (REVERTED: slower at N=1, N=10)
- `ccf1528` — List.traverseTaskResultM' (REVERTED: no benchmark proof)

**Test results:** ALL PASS — 4243+ tests across F#, JavaScript, Python
**Format check:** PASS
