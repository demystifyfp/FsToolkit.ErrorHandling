## AsyncResultOption.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Async<Result<('a -> 'b) option, 'c>>
  -> Async<Result<'a option, 'c>> 
  -> Async<Result<'b option, 'c>>
```

## Examples
