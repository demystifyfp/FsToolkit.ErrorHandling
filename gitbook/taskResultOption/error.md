## TaskResultOption.error

Namespace: `FsToolkit.ErrorHandling`

Lift an `'error` value into an `Task<Result<'ok option, 'error>>`

## Function Signature:

```fsharp
'error -> Task<Result<'ok option, 'error>>
```

## Examples

### Example 1


```fsharp
let result : Task<Result<int option, string>> =
  TaskResultOption.error "Something bad happened"
```

