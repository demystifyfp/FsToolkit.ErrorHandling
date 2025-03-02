# TaskValidation.zip

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
TaskValidation<'leftOk, 'error> -> TaskValidation<'rightOk, 'error> -> TaskValidation<'leftOk * 'rightOk, 'error>
```

## Examples

### Example 1

```fsharp
let result = TaskValidation.zip (TaskValidation.ok 1) (TaskValidation.ok 2)
// task { Ok (1, 2) }
```

### Example 2

```fsharp
let result = TaskValidation.zip (TaskValidation.ok 1) (TaskValidation.error "Bad")
// task { Error [ "Bad" ] }
```

### Example 3

```fsharp
let result = TaskValidation.zip (TaskValidation.error "Bad1") (TaskValidation.error "Bad2")
// task { Error [ "Bad1"; "Bad2" ] }
```
