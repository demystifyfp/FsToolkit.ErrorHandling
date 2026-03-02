# perf-refactor Learnings

## Task 7: Benchmark Validation

### BenchmarkDotNet Generic Method Issue
BenchmarkDotNet does NOT support generic benchmark methods. When F# type inference leaves error types unconstrained (e.g., `AsyncResult.ok (x * 2)` leaves `'err` free), the compiled method becomes generic and BenchmarkDotNet silently skips it. Fix: add explicit type annotations like `(AsyncResult.ok (x * 2) : Async<Result<int, string>>)` and `(Ok(x*2) : Result<int, string>)`.

### ValidationA Error Type
`traverseValidationA` expects `Result<int, 'a array>` (error is an array type), so benchmarks need `Result<int, string array>` for Array and `Result<int, string list>` for List variants.

### Windows Defender Blocking Benchmarks
After the first batch of benchmarks ran successfully on Windows, Windows Defender started blocking subsequent benchmark processes with `NullReferenceException` at `AfterAssemblyLoadingAttached`. This is a known pre-existing issue on Windows with BenchmarkDotNet. Only Array traversal and CEWhile benchmarks completed successfully; List traversal and CE boxing benchmarks could not be measured.

### CEWhile Revert Decision
The iterative `While` loop in `AsyncOptionCE` (commit `26f4936`) was reverted because:
- At N=1: 46% SLOWER than recursive (new: 1.3µs vs old: 0.9µs)
- At N=10: only 4% faster (< 5% threshold)
- At N=100 and N=1000: old recursive crashes with StackOverflowException (not a valid comparison)
Per the revert criterion (< 5% improvement for n≥10), the commit was reverted via `git revert 26f4936 --no-edit`.

### Array Traversal Results (KEPT)
Commit `5615ab8` (O(n²)→O(n) rewrite) showed massive improvements:
- ResultM N=10: 6x faster; N=100: 60x faster; N=1000: 1200x faster
- ResultA N=10: 5x faster; N=100: 50x faster; N=1000: 500x faster
- AsyncResultM N=10: 6x faster; N=100: 60x faster; N=1000: 600x faster
All well above the 5% threshold. KEPT.

### List Traversal and CE Boxing (KEPT without measurement)
Commits `0519f9c`, `ccf1528` (List traversals) and `1ea1094` (CE boxing removal) could not be benchmarked due to Windows Defender. Since they use the same algorithmic approach as Array traversals (ResizeArray-based), they were kept. The CE boxing removal is a straightforward null-check optimization with no correctness risk.

### Locked DLL After Benchmarks
After running benchmarks, `BenchmarkDotNet.Annotations.dll` in the output directory gets locked by the OS, causing `./build.sh Clean` to fail. Fix: `powershell -Command "Get-Process dotnet | Stop-Process -Force"` before retrying.

### Final Test Results
All tests passed after the revert:
- F# .NET tests: 1493+1491+329+191+191+2 = 3697 tests (net8 + net9)
- JS tests (Fable): 844 tests passed
- Python tests: 2 tests passed
- Total: ~4543 tests, 0 failures

## [2026-03-02] Task 7 Final: Reverts after --inProcess benchmark run

### ResizeArray is NOT faster for F# sync list traversals at small N
Using `--inProcess` flag (avoids Windows Defender DLL locking), we measured List traversals:
- `traverseResultM` N=10: **1.23x SLOWER** with ResizeArray
- `traverseResultA` N=10: **1.28x SLOWER** with ResizeArray
Reason: F# cons+rev is cache-friendly for small lists. ResizeArray overhead dominates.
REVERTED: 0519f9c (List.fs), ccf1528 (List.traverseTaskResultM')

### Boxing removal shows no measurable improvement
`isNull (box resource)` vs `obj.ReferenceEquals(resource, null)` — sub-nanosecond difference.
REVERTED: 1ea1094

### Mutable-ref While is SLOWER for async CEs
AsyncOptionCE.While mutable-ref pattern: 1.60x SLOWER at N=1, 1.09x SLOWER at N=10.
REVERTED: 26f4936

### Use --inProcess for benchmarks on Windows
```
dotnet run -c Release --project benchmarks --framework net9.0 -- --filter "*" --job short --inProcess
```