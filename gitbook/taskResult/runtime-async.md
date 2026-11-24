# Runtime-async `taskResult` (preview)

On .NET 11, the runtime can suspend and resume a `taskResult` computation. Runtime async is an F# preview feature. A later compiler can change its requirements or behavior.

Target `net11.0` and set `LangVersion` to `preview` in the project that uses the builder.

```xml
<PropertyGroup>
  <TargetFramework>net11.0</TargetFramework>
  <LangVersion>preview</LangVersion>
</PropertyGroup>
```

Open the runtime-async namespace after `FsToolkit.ErrorHandling`.

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

Only `taskResult` uses runtime async in this namespace. Modules that open only `FsToolkit.ErrorHandling` use the existing builder.

## First-call cost

On .NET 11.0.0, the first call to a large computation can spend seconds compiling it before reaching the first await. This cost grows with the number of separate binds in one method. If startup latency matters, measure the first call with a representative computation.

## Sources package

If you use `FsToolkit.ErrorHandling.Sources`, also add this property to the preview target:

```xml
<DefineConstants>$(DefineConstants);FSTK_RUNTIME_ASYNC</DefineConstants>
```

Use your configured source namespace in both `open` declarations. Without the symbol, the Sources package omits the runtime-async builder.
