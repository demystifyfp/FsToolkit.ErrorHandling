# AsyncResult.ofResult

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Result<'T, 'Error>` into an `Async<Result<'T, 'Error>>`.

## Function Signature

```fsharp
Result<'T, 'Error> -> Async<Result<'T, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = AsyncResult.ofResult (Ok 42)
// async { return Ok 42 }
```

### Example 2

```fsharp
let result = AsyncResult.ofResult (Error "Boom!")
// async { return Error "Boom!" }
```
