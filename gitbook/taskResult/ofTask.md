# TaskResult.ofTask

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Task<'T>` into a `Task<Result<'T, exn>>`.

## Function Signature

```fsharp
Task<'T> -> Task<Result<'T, exn>>
```

## Examples

### Example 1

```fsharp
let result = TaskResult.ofTask (task { return 42 })
// task { return Ok 42 }
```

### Example 2

```fsharp
let result = TaskResult.ofTask (task { return System.Exception("error") })
// task { return Error (System.Exception("error")) }
```
