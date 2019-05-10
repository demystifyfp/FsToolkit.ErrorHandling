## JobResult.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c -> 'd)
  -> Job<Result<'a, 'e>>
  -> Job<Result<'b, 'e>>
  -> Job<Result<'c, 'e>>
  -> Job<Result<'d, 'e>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `jobResult` computation expression](../jobResult/ce.md).