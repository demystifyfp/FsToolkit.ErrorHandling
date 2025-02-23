# TaskResult.zipError

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
Task<Result<'ok, 'leftError>> -> Task<Result<'ok, 'rightError>> -> Task<Result<'ok, 'leftError * 'rightError>>
```

## Examples

### Example 1

```fsharp
let result = TaskResult.zipError (TaskResult.ok 1) (TaskResult.ok 2)
// task { Ok 1 }
```

### Example 2

```fsharp
let result = TaskResult.zipError (TaskResult.ok 1) (TaskResult.error "Bad")
// task { Ok 1 }
```

### Example 3

```fsharp
let result = TaskResult.zipError (TaskResult.error "Bad1") (TaskResult.error "Bad2")
// task { Error("Bad1", "Bad2") }
```
