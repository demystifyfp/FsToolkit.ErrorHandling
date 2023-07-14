# ResultOption.ofChoice

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Choice<'T, 'Error>` value to `Result<'T option, 'Error>`.

## Function Signature

```fsharp
Choice<'T, 'Error> -> Result<'T option, 'Error>
```

## Examples

### Example 1

```fsharp
let result = ResultOption.ofChoice (Choice1Of2 42)
// Ok (Some 42)
```

### Example 2

```fsharp
let result = ResultOption.ofChoice (Choice2Of2 "error")
// Error "error"
```

