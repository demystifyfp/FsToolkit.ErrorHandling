## CancellableTaskValidation.ofResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Result<'a, 'b> -> CancellableTask<Result<'a, 'b list>>
```

Simply wraps the error in a list and makes the result a cancellable task.

## Examples
