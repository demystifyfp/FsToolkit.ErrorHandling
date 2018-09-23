## AsyncResult.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
Async<Result<('a -> 'b), 'c>> -> Async<Result<'a, 'c>> 
  -> Async<Result<'b, 'c>>
```