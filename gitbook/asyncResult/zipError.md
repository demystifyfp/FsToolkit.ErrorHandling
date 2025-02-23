# AsyncResult.zipError

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
Async<Result<'ok, 'leftError>> -> Async<Result<'ok, 'rightError>> -> Async<Result<'ok, 'leftError * 'rightError>>
```

## Examples

### Example 1

```fsharp
let result = AsyncResult.zipError (AsyncResult.ok 1) (AsyncResult.ok 2)
// async { Ok 1 }
```

### Example 2

```fsharp
let result = AsyncResult.zipError (AsyncResult.ok 1) (AsyncResult.error "Bad")
// async { Ok 1 }
```

### Example 3

```fsharp
let result = AsyncResult.zipError (AsyncResult.error "Bad1") (AsyncResult.error "Bad2")
// async { Error("Bad1", "Bad2") }
```
