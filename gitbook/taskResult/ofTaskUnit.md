# TaskResult.ofTaskUnit

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Task` into an `Task<Result<unit, 'Error>>`, trapping any Exception as the `Error` case.

## Function Signature

```fsharp
Task -> Task<Result<unit, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = TaskResult.ofTaskUnit (Task.Delay 1000)
// task { return Ok () }
```

### Example 2

```fsharp
let result = TaskResult.ofTaskUnit (Task.FromException (System.Exception("Boom!")))
// task { return Error (System.Exception("Boom!")) }
```
