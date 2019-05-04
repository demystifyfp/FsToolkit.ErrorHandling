## TaskResult.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c -> 'd)
  -> Task<Result<'a, 'e>>
  -> Task<Result<'b, 'e>>
  -> Task<Result<'c, 'e>>
  -> Task<Result<'d, 'e>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `taskResult` computation expression](../taskResult/ce.md).