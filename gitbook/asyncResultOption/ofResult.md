# AsyncResultOption.ofResult

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Result<'T, 'Error>` into an `Async<Result<'T option, 'Error>>`.

## Function Signature

```fsharp
Result<'T, 'Error> -> Async<Result<'T option, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = AsyncResultOption.ofResult (Ok 42)
// async { return Ok (Some 42) }
```

### Example 2

```fsharp
let result = AsyncResultOption.ofResult (Error "error")
// async { return Error "error" }
```
