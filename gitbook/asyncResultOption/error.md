## AsyncResultOption.error

Namespace: `FsToolkit.ErrorHandling`

Lift an `'error` value into an `Async<Result<'ok option, 'error>>`

## Function Signature:

```fsharp
'error -> Async<Result<'ok option, 'error>>
```

## Examples

### Example 1


```fsharp
let result : Async<Result<int option, string>> =
  AsyncResultOption.error "Something bad happened"
```

