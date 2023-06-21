## AsyncValidation.ofResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Result<'a, 'b> -> Async<Result<'a, 'b list>>
```

Simply wraps the error in a list and makes the result async.

## Examples
