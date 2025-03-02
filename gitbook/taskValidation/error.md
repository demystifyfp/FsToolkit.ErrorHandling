## TaskValidation.error

Namespace: `FsToolkit.ErrorHandling`

Lift an `'error` value into an `Task<Validation<'ok, 'error>>`

## Function Signature:

```fsharp
'error -> Task<Validation<'ok, 'error>>
```

## Examples

### Example 1


```fsharp
let result : Task<Validation<int, string>> =
  TaskValidation.error "Something bad happened"
```

