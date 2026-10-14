# TaskResult.ofTask

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Task<'T>` into a `Task<Result<'T, 'Error>>`.

## Function Signature

```fsharp
Task<'T> -> Task<Result<'T, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = TaskResult.ofTask (Task.result 42)
// task { return Ok 42 }
```

### Example 2

```fsharp
let result = TaskResult.ofTask (Task.FromException (System.Exception("Boom!")))
// task { return Error (System.Exception("Boom!")) }
```
