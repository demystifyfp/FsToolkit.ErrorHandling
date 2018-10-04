## AsyncResultOption.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b -> 'c -> 'd) -> Async<Result<'a option, 'e>> -> Async<Result<'b option, 'e>> 
  -> Async<Result<'c option, 'e>> -> Async<Result<'d option, 'e>>
```