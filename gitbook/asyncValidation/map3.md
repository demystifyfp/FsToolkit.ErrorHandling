## AsyncValidation.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b -> 'c -> 'd)
  -> Async<Result<'a, 'e list>>
  -> Async<Result<'b, 'e list>>
  -> Async<Result<'c, 'e list>>
  -> Async<Result<'d, 'e list>>
```

Like [Result.map3](../result/map3.md), but collects the errors from all arguments.

## Examples
