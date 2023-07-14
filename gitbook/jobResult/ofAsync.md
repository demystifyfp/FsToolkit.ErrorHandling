# JobResult.ofAsync

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Async<'T>` into an `Job<Result<'T, exn>>`.

## Function Signature

```fsharp
Async<'T> -> Job<Result<'T, exn>>
```

## Examples

### Example 1

```fsharp
let result = JobResult.ofAsync (async { return 42 })
// job { return Ok 42 }
```

### Example 2

```fsharp
let result = JobResult.ofAsync (async { return System.Exception("error") })
// job { return Error System.Exception("error") }
```
