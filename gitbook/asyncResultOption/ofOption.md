# AsyncResultOption.ofOption

Namespace: `FsToolkit.ErrorHandling`

Transforms a `'T option` into an `Async<Result<'T option, 'Error>>`.

## Function Signature

```fsharp
'T option -> Async<Result<'T option, 'Error>>
```

## Examples

### Example 1

```fsharp
let result = AsyncResultOption.ofOption (Some 42)
// async { return Ok (Some 42) }
```

### Example 2

```fsharp
let result = AsyncResultOption.ofOption None
// async { return Ok None }
```
