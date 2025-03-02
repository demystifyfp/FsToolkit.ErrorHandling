## TaskValidation.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b -> 'c -> 'd)
  -> Task<Result<'a, 'e list>>
  -> Task<Result<'b, 'e list>>
  -> Task<Result<'c, 'e list>>
  -> Task<Result<'d, 'e list>>
```

Like [Result.map3](../result/map3.md), but collects the errors from all arguments.

## Examples
