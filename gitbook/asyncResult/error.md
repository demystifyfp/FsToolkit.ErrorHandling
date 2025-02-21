## AsyncResult.error

Namespace: `FsToolkit.ErrorHandling`

Lift an 'error value into an Async<Result<'ok, 'error>>

## Function Signature:

```fsharp
'error -> Async<Result<'ok, 'error>>
```

## Examples

### Example 1


```fsharp
let result : Async<Result<int, string>> =
  AsyncResult.error "Something bad happened"
```

