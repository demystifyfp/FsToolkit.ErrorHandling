# AsyncValidation.zip

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
AsyncValidation<'leftOk, 'error> -> AsyncValidation<'rightOk, 'error> -> AsyncValidation<'leftOk * 'rightOk, 'error>
```

## Examples

### Example 1

```fsharp
let result = AsyncValidation.zip (AsyncValidation.ok 1) (AsyncValidation.ok 2)
// async { Ok (1, 2) }
```

### Example 2

```fsharp
let result = AsyncValidation.zip (AsyncValidation.ok 1) (AsyncValidation.error "Bad")
// async { Error [ "Bad" ] }
```

### Example 3

```fsharp
let result = AsyncValidation.zip (AsyncValidation.error "Bad1") (AsyncValidation.error "Bad2")
// async { Error [ "Bad1"; "Bad2" ] }
```
