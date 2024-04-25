# Result.zip

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
Result<'leftOk, 'error> -> Result<'rightOk, 'error> -> Result<'leftOk * 'rightOk, 'error>
```

## Examples

### Example 1

```fsharp
let result = Result.zip (Ok 1) (Ok 2)
// Ok (1, 2)
```

### Example 2

```fsharp
let result = Result.zip (Ok 1) (Error "Bad")
// Error "Bad"
```

### Example 3

```fsharp
let result = Result.zip (Error "Bad1") (Error "Bad2")
// Error "Bad1"
```
