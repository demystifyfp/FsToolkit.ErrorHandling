## CancellableTaskResult.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
CancellableTask<Result<('a -> 'b), 'c>>
  -> CancellableTask<Result<'a, 'c>>
  -> CancellableTask<Result<'b, 'c>>
```

## Examples
