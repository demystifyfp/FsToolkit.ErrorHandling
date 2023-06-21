## AsyncValidation.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c)
  -> Async<Result<'a, 'd list>>
  -> Async<Result<'b, 'd list>>
  -> Async<Result<'c, 'd list>>
```

Like [Result.map2](../result/map2.md), but collects the errors from both arguments.

## Examples
