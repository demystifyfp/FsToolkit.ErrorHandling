# TaskValidation.ofChoice

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Choice<'T, 'Error>` into a `Task<Result<'T, 'Error list>>`

## Function Signature

```fsharp
Choice<'T, 'Error> -> Task<Result<'T, 'Error list>>
```

## Examples

### Example 1

```fsharp
let result = TaskValidation.ofChoice (Choice1Of2 42)
// task { return Ok 42 }
```

### Example 2

```fsharp
let result = TaskValidation.ofChoice (Choice2Of2 "error")
// task { return Error ["error"] }
```
