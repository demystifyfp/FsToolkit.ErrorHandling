# AsyncResult.ofAsync

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Async<'T>` into an `Async<Result<'T, 'Error>>`.

## Function Signature

```fsharp
Async<'T> -> Async<Result<'T, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = AsyncResult.ofAsync (async { return 42 })
// async { return Ok 42 }
```
