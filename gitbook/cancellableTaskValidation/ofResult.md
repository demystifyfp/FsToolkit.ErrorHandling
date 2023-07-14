# CancellableTaskValidation.ofResult

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Result<'T, 'Error>` into a `CancellableTask<Result<'T, 'Error list>>`

## Function Signature

```fsharp
Result<'T, 'Error> -> CancellableTask<Result<'T, 'Error list>>
```

## Examples

### Example 1

```fsharp
let result = CancellableTaskValidation.ofResult (Ok 42)
// cancellableTask { return Ok 42 }
```

### Example 2

```fsharp
let result = CancellableTaskValidation.ofResult (Error "error")
// cancellableTask { return Error ["error"] }
```
