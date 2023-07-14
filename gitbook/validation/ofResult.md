# Validation.ofResult

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Result<'T, 'Error>` into a `Result<'T, 'Error list>`

## Function Signature

```fsharp
Result<'T, 'Error> -> Result<'T, 'Error list>
```

## Examples

### Example 1

```fsharp
let result = Validation.ofResult (Ok 42)
// Ok 42
```

### Example 2

```fsharp
let result = Validation.ofResult (Error "error")
// Error ["error"]
```
