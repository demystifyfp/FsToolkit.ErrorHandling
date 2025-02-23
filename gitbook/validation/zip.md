# Validation.zip

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
Validation<'leftOk, 'error> -> Validation<'rightOk, 'error> -> Validation<'leftOk * 'rightOk, 'error>
```

## Examples

### Example 1

```fsharp
let result = Validation.zip (Validation.ok 1) (Validation.ok 2)
// Ok (1, 2)
```

### Example 2

```fsharp
let result = Validation.zip (Validation.ok 1) (Validation.error "Bad")
// Error [ "Bad" ]
```

### Example 3

```fsharp
let result = Validation.zip (Validation.error "Bad1") (Validation.error "Bad2")
// Error [ "Bad1"; "Bad2" ]
```
