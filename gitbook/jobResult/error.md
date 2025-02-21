## JobResult.error

Namespace: `FsToolkit.ErrorHandling`

Lift an `'error` value into an `Job<Result<'ok, 'error>>`

## Function Signature:

```fsharp
'error -> Job<Result<'ok, 'error>>
```

## Examples

### Example 1


```fsharp
let result : Job<Result<int, string>> =
  JobResult.error "Something bad happened"
```

