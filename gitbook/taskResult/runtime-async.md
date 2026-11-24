# Runtime-async task builders (preview)

On .NET 11, the runtime can suspend and resume FsToolkit task computations. Runtime async is an F# preview feature. A later compiler can change its requirements or behavior.

Target `net11.0` and set `LangVersion` to `preview` in the project that uses the builder.

```xml
<PropertyGroup>
  <TargetFramework>net11.0</TargetFramework>
  <LangVersion>preview</LangVersion>
</PropertyGroup>
```

Open `FsToolkit.ErrorHandling.Polyfill.RuntimeTask` after `FsToolkit.ErrorHandling`. If you also use the IcedTasks runtime-async builders, open `IcedTasks.Polyfill.RuntimeTask` before the FsToolkit runtime-async namespace.

```fsharp
open System.Threading.Tasks
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Polyfill.RuntimeTask

let doubleValue (input: Task<Result<int, string>>) =
    taskResult {
        let! value = input
        return value * 2
    }
```

An `Ok 21` input gives `Ok 42`. An `Error` input skips the multiplication and returns the same error. The return type is `Task<Result<int, string>>`, so you can pass it to `TaskResult.map`.

After a loop body returns an `Error`, the preview builder disposes the enumerator without advancing it again. A `while` loop also stops without calling its guard again. The existing builder can call the guard or advance a synchronous enumerator once more.

## Builders

Modules that open only `FsToolkit.ErrorHandling` use the existing builders. The opt-in builders use the same result types as their existing counterparts.

| Package | Builder | Background variant |
| --- | --- | --- |
| `FsToolkit.ErrorHandling` | `taskResult` | `backgroundTaskResult` |
| | `taskOption` | `backgroundTaskOption` |
| | `taskValueOption` | `backgroundTaskValueOption` |
| | `taskResultOption` | |
| | `taskValidation` | `backgroundTaskValidation` |
| `FsToolkit.ErrorHandling.IcedTasks` | `cancellableTaskResult` | `backgroundCancellableTaskResult` |
| | `cancellableTaskOption` | `backgroundCancellableTaskOption` |
| | `cancellableTaskValidation` | |
| | `cancellableValueTaskResult` | `backgroundCancellableValueTaskResult` |
| | `cancellableValueTaskOption` | `backgroundCancellableValueTaskOption` |
| | `valueTaskValueOption` | |

The IcedTasks package uses `IcedTasks` 0.12.0-beta002. Apply a cancellable computation to a `CancellationToken` to start it. If the token is already canceled, the body does not start. Background builders move work to the thread pool when the caller has a synchronization context or a custom task scheduler. A cancellable background builder checks the context and scheduler when you supply the token.

In an asynchronous `for` loop, the cancellable builders await any move that has started. If the token is canceled before the move returns an item, the preview builders dispose the enumerator and skip the loop body. With a bind-free body, the existing builders can continue until the enumerator stops supplying items.

As with FSharp.Core `task` and IcedTasks `cancellableTask`, an `AsyncLocal` assignment inside a loop remains visible after the loop in the same invocation. The caller keeps its original value.

Projects that explicitly enable the optional F# warning `FS3517` can get this warning in a cancellable result expression. For example, an `if` followed by a `return` can trigger the warning on the first argument of `Combine`. A project that treats this warning as an error cannot compile that expression.

## First-call cost

On .NET 11.0.0, the first call to a large computation can spend seconds compiling it before reaching the first await. This cost grows with the number of separate binds in one method. If startup latency matters, measure the first call with a representative computation.

## Single-bind benchmark

Two BenchmarkDotNet runs compared one `taskResult` bind on .NET 11.0.0 for Windows x64. Each time column shows the two means in nanoseconds. A pending input completed after the builder returned.

| Input | Existing (ns) | Runtime async (ns) | Existing (bytes) | Runtime async (bytes) |
| --- | ---: | ---: | ---: | ---: |
| Completed `Ok` | 33.87 / 36.54 | 24.56 / 24.79 | 80 | 80 |
| Completed `Error` | 45.47 / 45.83 | 24.76 / 23.08 | 80 | 80 |
| Pending `Ok` | 115.97 / 96.86 | 139.03 / 138.28 | 208 | 288 |
| Pending `Error` | 114.26 / 81.78 | 146.24 / 102.18 | 208 | 288 |

A separate .NET 11 Release test measured 100,000 one-bind `cancellableValueTaskResult` calls after warm-up. For a completed `ValueTask`, the existing builder allocated 136 bytes per call and the preview builder allocated 104 bytes. For a pending `Task`, the totals were 448 and 408 bytes. Both totals include the `TaskCompletionSource` used for the pending input.

## Sources package

If you use `FsToolkit.ErrorHandling.Sources`, also add this property to the preview target:

```xml
<Features>$(Features);runtime-async=on</Features>
```

Use your configured source namespace in both `open` declarations. Without this feature setting, the Sources package omits the core runtime-async builders. The setting uses the .NET compiler feature name. [Runtime async in .NET 11](https://github.com/dotnet/docs/blob/main/docs/core/whats-new/dotnet-11/runtime.md#runtime-async)
