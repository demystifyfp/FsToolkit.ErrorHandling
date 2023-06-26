## CancellableTaskResult.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c -> 'd)
  -> CancellableTask<Result<'a, 'e>>
  -> CancellableTask<Result<'b, 'e>>
  -> CancellableTask<Result<'c, 'e>>
  -> CancellableTask<Result<'d, 'e>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `cancellableTaskResult` computation expression](../cancellableTaskResult/ce.md).
