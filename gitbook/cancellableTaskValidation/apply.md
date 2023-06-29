## CancellableTaskValidation.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
CancellableTask<Result<('a -> 'b), 'c list>>
  -> CancellableTask<Result<'a, 'c list>>
  -> CancellableTask<Result<'b, 'c list>>
```

## Examples
