# CancellableTaskValidation.ofResult

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Choice<'T, 'Error>` into a `CancellableTask<Result<'T, 'Error list>>`

## Function Signature

```fsharp
Choice<'T, 'Error> -> CancellableTask<Result<'T, 'Error list>>
```

## Examples

### Example 1

```fsharp
let result = CancellableTaskValidation.ofChoice (Choice1Of2 42)
// cancellableTask { return Ok 42 }
```

### Example 2

```fsharp
let result = CancellableTaskValidation.ofChoice (Choice2Of2 "error")
// cancellableTask { return Error ["error"] }
```
