# JobResult.ofJob

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Job<'T>` into an `Job<Result<'T, 'Error>>`.

## Function Signature

```fsharp
Job<'T> -> Job<Result<'T, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = JobResult.ofJob (job { return 42 })
// job { return Ok 42 }
```
