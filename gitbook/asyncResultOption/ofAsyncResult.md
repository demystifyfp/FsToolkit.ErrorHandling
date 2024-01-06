# AsyncResultOption.ofAsyncResult

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Async<Result<'T, 'Error>>` into an `Async<Result<'T option, 'Error>>`.

## Function Signature

```fsharp
Async<Result<'T, 'Error>> -> Async<Result<'T option, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = AsyncResultOption.ofAsyncResult (async { return Ok 42 })
// async { return Ok (Some 42) }
```

### Example 2

```fsharp
let result = AsyncResultOption.ofAsyncResult (async { return Error "error" })
// async { return Error "error" }
```
