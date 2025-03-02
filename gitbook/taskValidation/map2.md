## TaskValidation.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c)
  -> Task<Result<'a, 'd list>>
  -> Task<Result<'b, 'd list>>
  -> Task<Result<'c, 'd list>>
```

Like [Result.map2](../result/map2.md), but collects the errors from both arguments.

## Examples
