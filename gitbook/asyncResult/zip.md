# AsyncResult.zip

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
Async<Result<'leftOk, 'error>> -> Async<Result<'rightOk, 'error>> -> Async<Result<'leftOk * 'rightOk, 'error>>
```

## Examples

### Example 1

```fsharp
let result = AsyncResult.zip (AsyncResult.ok 1) (AsyncResult.ok 2)
// async { Ok (1, 2) }
```

### Example 2

```fsharp
let result = AsyncResult.zip (AsyncResult.ok 1) (AsyncResult.error "Bad")
// async { Error "Bad" }
```

### Example 3

```fsharp
let result = AsyncResult.zip (AsyncResult.error "Bad1") (AsyncResult.error "Bad2")
// async { Error "Bad1" }
```
