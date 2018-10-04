## AsyncResultOption.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
Async<Result<('a -> 'b) option, 'c>> -> Async<Result<'a option, 'c>> 
  -> Async<Result<'b option, 'c>>
```