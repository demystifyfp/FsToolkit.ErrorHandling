# Wave 4: Task.apply + Task.mapV Benchmark Evidence

## Task.apply Results

Old: `bind (fun f' -> bind (fun x' -> singleton (f' x')) x) f`
New: `task { let! f' = f; let! x' = x; return f' x' }`

| Method        | Mean     | Ratio        | Allocated | Alloc Ratio |
|-------------- |---------:|-------------:|----------:|------------:|
| Old_TaskApply | 47.61 ns |     baseline |     264 B |             |
| New_TaskApply | 14.78 ns | 3.22x faster |      72 B |  3.67x less |

**VERDICT: KEEP** - 3.22x faster, 3.67x less allocation

## Task.mapV Results

Old: `x |> bindV (f >> singleton)`
New: `task { let! v = x; return f v }` (with explicit `ValueTask<'a>` annotation)

| Method       | Mean     | Ratio        | Allocated | Alloc Ratio |
|------------- |---------:|-------------:|----------:|------------:|
| Old_TaskMapV | 23.49 ns |     baseline |     168 B |             |
| New_TaskMapV | 12.16 ns | 1.93x faster |      72 B |  2.33x less |

**VERDICT: KEEP** - 1.93x faster, 2.33x less allocation

## Why This Works

Both optimizations eliminate the same anti-pattern: `bind (f >> singleton)` or nested `bind` calls.

1. **Function composition allocation**: `f >> singleton` creates a new FSharpFunc object
2. **Bind+ReturnFrom vs BindReturn**: The old pattern goes through `Bind` then `ReturnFrom` (two state machine transitions). Direct `task { let! v = x; return f v }` uses the optimized `BindReturn` path (one transition).
3. **Nested closures in apply**: The old `apply` had TWO nested lambdas capturing `f`, `x`, and `f'`, creating 3 closure allocations. Direct CE creates only the state machine.

## Environment

- BenchmarkDotNet v0.15.0
- .NET 9.0.13, X64 RyuJIT AVX2
- 12th Gen Intel Core i9-12900F
- ShortRun, InProcess
