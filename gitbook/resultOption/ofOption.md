# ResultOption.ofOption

Namespace: `FsToolkit.ErrorHandling`

Transforms a `'T Option` value to `Result<'T option, 'Error>`.

## Function Signature

```fsharp
Result<'T, 'Error> -> Result<'T option, 'Error>
```

## Examples

### Example 1

```fsharp
let result = ResultOption.ofOption (Some 42)
// Ok (Some 42)
```

### Example 2

```fsharp
let result = ResultOption.ofOption None
// Ok None
```

