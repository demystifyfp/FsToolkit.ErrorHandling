# TaskResult.zip

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
Task<Result<'leftOk, 'error>> -> Task<Result<'rightOk, 'error>> -> Task<Result<'leftOk * 'rightOk, 'error>>
```

## Examples

### Example 1

```fsharp
let result = TaskResult.zip (TaskResult.ok 1) (TaskResult.ok 2)
// task { Ok (1, 2) }
```

### Example 2

```fsharp
let result = TaskResult.zip (TaskResult.ok 1) (TaskResult.error "Bad")
// task { Error "Bad" }
```

### Example 3

```fsharp
let result = TaskResult.zip (TaskResult.error "Bad1") (TaskResult.error "Bad2")
// task { Error "Bad1" }
```
