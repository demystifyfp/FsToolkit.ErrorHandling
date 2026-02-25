## JobResultOption.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Job<Result<('a -> 'b) option, 'c>>
  -> Job<Result<'a option, 'c>>
  -> Job<Result<'b option, 'c>>
```

## Examples
