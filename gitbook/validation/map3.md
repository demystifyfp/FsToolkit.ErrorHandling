## Validation.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b -> 'c -> 'd)
  -> Result<'a, 'e list>
  -> Result<'b, 'e list>
  -> Result<'c, 'e list>
  -> Result<'d, 'e list>
```

Like [Result.map3](../result/map3.md), but collects the errors from all arguments.

## Examples
