# Validation.ofChoice

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Choice<'T, 'Error>` into a `Result<'T, 'Error list>`

## Function Signature

```fsharp
Choice<'T, 'Error> -> Result<'T, 'Error list>
```

## Examples

### Example 1

```fsharp
let result = Validation.ofChoice (Choice1Of2 42)
// Ok 42
```

### Example 2

```fsharp
let result = Validation.ofChoice (Choice2Of2 "error")
// Error ["error"]
```
