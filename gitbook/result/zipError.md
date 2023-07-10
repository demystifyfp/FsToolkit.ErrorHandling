# Result.zipError

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
Result<'ok, 'leftError> -> Result<'ok, 'rightError> -> Result<'ok, 'leftError * 'rightError>
```

## Examples

### Example 1

```fsharp
let result = Result.zip (Ok 1) (Ok 2)
// Ok  1
```

### Example 2

```fsharp
let result = Result.zip (Ok 1) (Error "Bad")
// Ok  1
```

### Example 3

```fsharp
let result = Result.zip (Error "Bad1") (Error "Bad2")
// Error("Bad1", "Bad2")
```
