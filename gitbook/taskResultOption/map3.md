## TaskResultOption.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c -> 'd)
  -> Task<Result<'a option, 'e>>
  -> Task<Result<'b option, 'e>> 
  -> Task<Result<'c option, 'e>>
  -> Task<Result<'d option, 'e>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `taskResultOption` computation expression](ce.md).
