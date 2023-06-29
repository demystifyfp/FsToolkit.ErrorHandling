## CancellableTaskValidation.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b -> 'c -> 'd)
  -> CancellableTask<Result<'a, 'e list>>
  -> CancellableTask<Result<'b, 'e list>>
  -> CancellableTask<Result<'c, 'e list>>
  -> CancellableTask<Result<'d, 'e list>>
```

Like [Result.map3](../result/map3.md), but collects the errors from all arguments.

## Examples
