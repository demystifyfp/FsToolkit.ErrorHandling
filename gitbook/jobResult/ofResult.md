# JobResult.ofResult

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Result<'T, 'Error>` into an `Job<Result<'T, 'Error>>`.

## Function Signature

```fsharp
Result<'T, 'Error> -> Job<Result<'T, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = JobResult.ofResult (Ok 42)
// job { return Ok 42 }
```

### Example 2

```fsharp
let result = JobResult.ofResult (Error "error")
// job { return Error "error" }
```
