# AsyncValidation.ofChoice

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Choice<'T, 'Error>` into a `Async<Result<'T, 'Error list>>`

## Function Signature

```fsharp
Choice<'T, 'Error> -> Async<Result<'T, 'Error list>>
```

## Examples

### Example 1

```fsharp
let result = AsyncValidation.ofChoice (Choice1Of2 42)
// async { return Ok 42 }
```

### Example 2

```fsharp
let result = AsyncValidation.ofChoice (Choice2Of2 "error")
// async { return Error ["error"] }
```
