## AsyncResultOption.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c -> 'd)
  -> Async<Result<'a option, 'e>>
  -> Async<Result<'b option, 'e>> 
  -> Async<Result<'c option, 'e>>
  -> Async<Result<'d option, 'e>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `asyncResultOption` computation expression](ce.md).