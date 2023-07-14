# ResultOption.ofResult

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Result<'T, 'Error>` value to `Result<'T option, 'Error>`.

## Function Signature

```fsharp
Result<'T, 'Error> -> Result<'T option, 'Error>
```

## Examples

### Example 1

```fsharp
let result = ResultOption.ofResult (Ok 42)
// Ok (Some 42)
```

### Example 2

```fsharp
let result = ResultOption.ofResult (Error "error")
// Error "error"
```

