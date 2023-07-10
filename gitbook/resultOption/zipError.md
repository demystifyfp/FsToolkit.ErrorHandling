# ResultOption.zipError

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Result<'ok option, 'leftError> -> Result<'ok option, 'rightError> -> Result<'ok option, 'leftError * 'rightError>
```

## Examples

### Example 1

```fsharp
let result = ResultOption.zip (Ok(Some 1)) (Ok(Some 2))
// Ok (Some 1)
```

### Example 2

```fsharp
let result = ResultOption.zip (Ok(Some 1)) (Ok None)
// Ok (Some 1)
```

### Example 3

```fsharp
let result = ResultOption.zip (Ok(Some 1)) (Error "Bad")
// Ok (Some 1)
```

### Example 4

```fsharp
let result = ResultOption.zip (Error "Bad1") (Error "Bad2")
// Error("Bad1", "Bad2")
```
