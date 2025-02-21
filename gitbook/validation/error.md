## Validation.error

Namespace: `FsToolkit.ErrorHandling`

Lift an `'error` value into an `Validation<'ok, 'error>`

## Function Signature:

```fsharp
'error -> Validation<'ok, 'error>
```

## Examples

### Example 1


```fsharp
let result : Validation<int, string> =
  Validation.error "Something bad happened"
```

