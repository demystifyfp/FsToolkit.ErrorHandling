# TaskResult.ofResult

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Result<'T, 'Error>` into a `Task<Result<'T, 'Error>>`.

## Function Signature

```fsharp
Result<'T, 'Error> -> Task<Result<'T, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = TaskResult.ofResult (Ok 42)
// task { return Ok 42 }
```

### Example 2

```fsharp
let result = TaskResult.ofResult (Error "error")
// task { return Error "error" }
```
