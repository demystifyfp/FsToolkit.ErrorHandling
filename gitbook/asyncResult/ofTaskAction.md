# AsyncResult.ofTaskAction

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Task` into an `Async<Result<unit, 'Error>>`.

## Function Signature

```fsharp
Task -> Async<Result<unit, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = AsyncResult.ofTaskAction (Task.Delay 1000)
// async { do () }
```

### Example 2

```fsharp
let result = AsyncResult.ofTaskAction (Task.FromException (System.Exception("Boom!")))
// async { return Error (System.Exception("Boom!")) }
```
