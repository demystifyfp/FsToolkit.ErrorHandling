## ParallelAsyncValidation.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c)
  -> Async<Result<'a, 'd list>>
  -> Async<Result<'b, 'd list>>
  -> Async<Result<'c, 'd list>>
```

Like [ParallelAsyncResult.map2](../parallelAsyncResult/map2.md), but collects the errors from both arguments.
