# AsyncResultOption.ofAsyncOption

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Async<'T option>` into an `Async<Result<'T option, 'Error>>`.

## Function Signature

```fsharp
Async<'T option> -> Async<Result<'T option, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = AsyncResultOption.ofAsyncOption (async { return Some 42 })
// async { return Ok (Some 42) }
```

### Example 2

```fsharp
let result = AsyncResultOption.ofAsyncOption (async { return None })
// async { return Ok None }
```
