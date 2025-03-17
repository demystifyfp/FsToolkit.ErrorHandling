## ParallelAsyncResult Computation Expression

Namespace: `FsToolkit.ErrorHandling`

This CE operates on the same type as `asyncResult`, but it adds the `and!` operator for running workflows in parallel.

When running concurrent workflows, fail-fast semantics are used. If any sub-task returns an `Error`, then all other tasks are cancelled and only that error is returned. To instead collect all errors, use `parallelAsyncValidation`.


## Examples

### Example 1

Suppose we want to download 3 files.

Here is our simulated download function:

```fsharp
// string -> Async<Result<string, string>>
let downloadAsync stuff : Async<Result<string, string>> = async {
    do! Async.Sleep 3_000
    return Ok stuff
}
```

This workflow will download each item in sequence:

```fsharp
let downloadAllSequential = ayncResult {
  let! x = downloadAsync (Ok "We")
  let! y = downloadAsync (Ok "run")
  let! z = downloadAsync (Ok "sequentially :(")
  return sprintf "%s %s %s" x y z
}
```

It takes 9 seconds to complete.

However, using `parallelAsyncResult`, we can download all 3 concurrently:

```fsharp
// Async<Result<string, string>>
let downloadAll = parallelAsyncResult {
  let! x = downloadAsync (Ok "We")
  and! y = downloadAsync (Ok "run")
  and! z = downloadAsync (Ok "concurrently!")
  return sprintf "%s %s %s" x y z
}
```

This takes just 3 seconds.
