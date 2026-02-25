# TaskValidation.ok

Namespace: `FsToolkit.ErrorHandling`

`ok` lifts a value into a successful `TaskValidation`.

## Function Signature

```fsharp
'ok -> TaskValidation<'ok, 'error>
```

## Examples

### Example 1

```fsharp
let result : TaskValidation<int, string> =
    TaskValidation.ok 42

// task { Ok 42 }
```

### Example 2

```fsharp
let result : TaskValidation<string, string> =
    TaskValidation.ok "hello"

// task { Ok "hello" }
```
