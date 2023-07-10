# ResultOption.zip

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Result<'leftOk option, 'error> -> Result<'rightOk option, 'error> -> Result<('leftOk * 'rightOk) option, 'error>
```

## Examples

### Example 1

```fsharp
let result = ResultOption.zip (Ok(Some 1)) (Ok(Some 2))
// Ok (Some(1, 2))
```

### Example 2

```fsharp
let result = ResultOption.zip (Ok(Some 1)) (Ok None)
// Ok None
```

### Example 3

```fsharp
let result = ResultOption.zip (Ok(Some 1)) (Error "Bad")
// Error "Bad"
```

### Example 4

```fsharp
let result = ResultOption.zip (Error "Bad1") (Error "Bad2")
// Error "Bad1"
```
