## TaskResultOption.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Task<Result<('a -> 'b) option, 'c>>
  -> Task<Result<'a option, 'c>> 
  -> Task<Result<'b option, 'c>>
```

## Examples
