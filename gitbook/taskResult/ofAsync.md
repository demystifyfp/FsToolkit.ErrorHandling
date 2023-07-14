# TaskResult.ofAsync

Namespace: `FsToolkit.ErrorHandling`

Transforms an `Async<'T>` into a `Task<Result<'T, exn>>`.

## Function Signature

```fsharp
Async<'T> -> Task<Result<'T, exn>>
```

## Examples

### Example 1

```fsharp
let result = TaskResult.ofAsync (async { return 42 })
// task { return Ok 42 }
```

### Example 2

```fsharp
let result = TaskResult.ofAsync (async { return System.Exception("error") })
// task { return Error (System.Exception("error")) }
```
