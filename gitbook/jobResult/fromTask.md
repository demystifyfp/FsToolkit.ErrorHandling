# JobResult.fromTask

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Task<'T>` into a `Job<Result<'T, Exception>>`

## Function Signature

```fsharp
Task<'T> -> Job<Result<'T, Exception>>
```

## Examples

### Example 1

```fsharp
let result = JobResult.fromTask (task { return 42 })
// job { return Ok 42 }
```

### Example 2

```fsharp
let result = JobResult.fromTask (task { return System.Exception("error") })
// job { return Error "error" }
```
