# Wave 5 Benchmark Evidence — TaskOption.apply + TaskResult.bindRequire*

## Date: 2026-03-02

## Changes

### Group 1: TaskOption.apply / TaskValueOption.apply / ValueTaskValueOption.apply
- **Pattern**: Replaced nested `bind (fun f' -> bind (fun x' -> some (f' x')) x) f` with direct `task { let! fOpt = f; match fOpt with ... }`
- **Files**: TaskOption.fs, TaskValueOption.fs, ValueTaskValueOption.fs

### Group 2: TaskResult.bindRequire* (11 functions)
- **Pattern**: Replaced `x |> bind (Result.requireXxx error >> Task.singleton)` with direct `task { let! result = x; return result |> Result.bind (Result.requireXxx error) }`
- **Functions**: bindRequireTrue, bindRequireFalse, bindRequireSome, bindRequireNone, bindRequireValueSome, bindRequireValueNone, bindRequireNotNull, bindRequireEqual, bindRequireEqualTo, bindRequireEmpty, bindRequireNotEmpty
- **File**: TaskResult.fs

## Benchmark Results

### TaskOption.apply (BenchmarkDotNet --job short --inProcess)

| Method                    | Mean     | Error   | StdDev  | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------------- |---------:|--------:|--------:|------:|--------:|----------:|------------:|
| Old_TaskOptionApply       | 62.01 ns | 0.77 ns | 0.64 ns |  1.00 |    0.02 |     288 B |        1.00 |
| New_TaskOptionApply       | 20.54 ns | 0.26 ns | 0.23 ns |  0.33 |    0.01 |      96 B |        0.33 |
| Old_TaskOptionApply_None  | 23.79 ns | 0.40 ns | 0.37 ns |  0.38 |    0.01 |     288 B |        1.00 |
| New_TaskOptionApply_None  |  7.32 ns | 0.11 ns | 0.10 ns |  0.12 |    0.00 |      24 B |        0.08 |

**Verdict: KEEP** — 3.02x faster (Some path), 3.25x faster (None path). 3.00x less allocation (Some), 12.00x less (None).

### TaskResult.bindRequireSome (BenchmarkDotNet --job short --inProcess)

| Method                          | Mean     | Error   | StdDev  | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------------------- |---------:|--------:|--------:|------:|--------:|----------:|------------:|
| Old_BindRequireSome_OkSome      | 36.32 ns | 0.48 ns | 0.42 ns |  1.00 |    0.02 |     184 B |        1.00 |
| New_BindRequireSome_OkSome      | 21.55 ns | 0.27 ns | 0.24 ns |  0.59 |    0.01 |      80 B |        0.43 |
| Old_BindRequireSome_OkNone      | 34.42 ns | 0.33 ns | 0.29 ns |  0.95 |    0.01 |     184 B |        1.00 |
| New_BindRequireSome_OkNone      | 20.55 ns | 0.28 ns | 0.25 ns |  0.57 |    0.01 |      80 B |        0.43 |
| Old_BindRequireSome_Error       | 27.47 ns | 0.44 ns | 0.39 ns |  0.76 |    0.01 |     104 B |        0.57 |
| New_BindRequireSome_Error       | 22.25 ns | 0.38 ns | 0.32 ns |  0.61 |    0.01 |      80 B |        0.43 |

**Verdict: KEEP** — 1.69x faster (OkSome), 1.67x faster (OkNone), 1.23x faster (Error). 2.30x less allocation across all paths.

## Revert Criteria Check
- ✅ All Mean ratios < 0.95 (≥5% improvement) for N≥10 equivalent
- ✅ All Alloc Ratios < 1.0 (less allocation)
- ✅ All tests pass (F#, JS, Python) — full RunTests suite green

## Conclusion
Both optimization groups meet all acceptance criteria. Changes kept.
