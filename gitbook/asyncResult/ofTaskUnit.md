# AsyncResult.ofTaskUnit

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Task` into an `Async<Result<unit, 'Error>>`, trapping any Exception as the `Error` case.

## Function Signature

```fsharp
Task -> Async<Result<unit, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = AsyncResult.ofTaskUnit (Task.Delay 1000)
// async { return Ok () }
```

### Example 2

```fsharp
let result = AsyncResult.ofTaskUnit (Task.FromException (System.Exception("Boom!")))
// async { return Error (System.Exception("Boom!")) }
```
