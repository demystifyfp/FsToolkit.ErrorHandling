# AsyncResult.ofTask

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Task<'T>` into an `Async<Result<'T, 'Error>>`.

## Function Signature

```fsharp
Task<'T> -> Async<Result<'T, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = AsyncResult.ofTask (Task.FromResult 42)
// async { return Ok 42 }
```

### Example 2

```fsharp
let result = AsyncResult.ofTask (Task.FromException (System.Exception("Boom!")))
// async { return Error (System.Exception("Boom!")) }
```
