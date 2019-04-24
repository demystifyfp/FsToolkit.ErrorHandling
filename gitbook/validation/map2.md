## Validation.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c)
  -> Result<'a, 'd list>
  -> Result<'b, 'd list> 
  -> Result<'c, 'd list>
```

Like [Result.map2](../result/map2.md), but collects the errors from both arguments.

## Examples