## CancellableTaskValidation.error

Namespace: `FsToolkit.ErrorHandling`

Lift an 'error value into an Async<Validation<'ok, 'error>>

## Function Signature:

```fsharp
'error -> Async<Validation<'ok, 'error>>
```

## Examples

### Example 1


```fsharp
let result : Async<Validation<int, string>> =
  AsyncValidation.error "Something bad happened"
```

