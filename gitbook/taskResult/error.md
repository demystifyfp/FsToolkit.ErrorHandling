## TaskResult.error

Namespace: `FsToolkit.ErrorHandling`

Lift an 'error value into an Task<Result<'ok, 'error>>

## Function Signature:

```fsharp
'error -> Task<Result<'ok, 'error>>
```

## Examples

### Example 1


```fsharp
let result : Task<Result<int, string>> =
  TaskResult.error "Something bad happened"
```

