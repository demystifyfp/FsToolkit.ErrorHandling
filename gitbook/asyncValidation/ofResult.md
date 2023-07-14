# AsyncValidation.ofResult

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Result<'T, 'Error>` into a `Async<Result<'T, 'Error list>>`

## Function Signature

```fsharp
Result<'T, 'Error> -> Async<Result<'T, 'Error list>>
```

## Examples

### Example 1

```fsharp
let result = AsyncValidation.ofResult (Ok 42)
// async { return Ok 42 }
```

### Example 2

```fsharp
let result = AsyncValidation.ofResult (Error "error")
// async { return Error ["error"] }
```
