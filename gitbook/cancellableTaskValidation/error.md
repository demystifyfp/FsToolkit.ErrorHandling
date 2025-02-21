## CancellableTaskValidation

Namespace: `FsToolkit.ErrorHandling`

Lift an `'error` value into an `CancellableTaskValidation<'ok, 'error>`

## Function Signature:

```fsharp
'error -> CancellableTaskValidation<'ok, 'error>
```

## Examples

### Example 1


```fsharp
let result : CancellableTaskValidation<int, string> =
  CancellableTaskValidation.error "Something bad happened"
```

