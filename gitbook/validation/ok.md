# Validation.ok

Namespace: `FsToolkit.ErrorHandling`

`ok` lifts a value into a successful `Validation`.

## Function Signature

```fsharp
'ok -> Validation<'ok, 'error>
```

## Examples

### Example 1

```fsharp
let result : Validation<int, string> =
    Validation.ok 42

// Ok 42
```

### Example 2

```fsharp
let result : Validation<string, string> =
    Validation.ok "hello"

// Ok "hello"
```
