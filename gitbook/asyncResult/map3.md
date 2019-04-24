## AsyncResult.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c -> 'd)
  -> Async<Result<'a, 'e>>
  -> Async<Result<'b, 'e>>
  -> Async<Result<'c, 'e>>
  -> Async<Result<'d, 'e>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `asyncResult` computation expression](../asyncResult/ce.md).