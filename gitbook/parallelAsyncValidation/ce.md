## ParallelAsyncValidation Computation Expression

Namespace: `FsToolkit.ErrorHandling`

This CE operates in the same way as `asyncValidation`, except that the `and!` operator will run workflows in parallel.

Concurrent workflows are run with the same semantics as [`Async.Parallel`](https://fsharp.github.io/fsharp-core-docs/reference/fsharp-control-fsharpasync.html#Parallel).


## Examples

See [here](../validation/ce.md) for other validation-like examples

```fsharp
// Result<string, string> -> Async<Result<string, string>>
let downloadAsync stuff = async {
    return stuff
}

// AsyncValidation<string, string>
let addResult = parallelAsyncValidation {
    let! x = downloadAsync (Ok "I")
    and! y = downloadAsync (Ok "am")
    and! z = downloadAsync (Ok "concurrent!")
    return sprintf "%s %s %s" x y z
}
// async { return Ok "I am concurrent!" }

// AsyncValidation<string, string>
let addResult = parallelAsyncValidation {
    let! x = downloadAsync (Error "Am")
    and! y = downloadAsync (Error "I")
    and! z = downloadAsync (Error "concurrent?")
    return sprintf "%s %s %s" x y z
}
// async { return Error [ "Am"; "I"; "concurrent?" ] }
```
