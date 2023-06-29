## CancellableTaskValidation.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c)
  -> CancellableTask<Result<'a, 'd list>>
  -> CancellableTask<Result<'b, 'd list>>
  -> CancellableTask<Result<'c, 'd list>>
```

Like [Result.map2](../result/map2.md), but collects the errors from both arguments.

## Examples
