# AsyncValidation.ok

Namespace: `FsToolkit.ErrorHandling`

`ok` lifts a value into a successful `AsyncValidation`.

## Function Signature

```fsharp
'ok -> AsyncValidation<'ok, 'error>
```

## Examples

### Example 1

```fsharp
let result : AsyncValidation<int, string> =
    AsyncValidation.ok 42

// async { Ok 42 }
```

### Example 2

```fsharp
let result : AsyncValidation<string, string> =
    AsyncValidation.ok "hello"

// async { Ok "hello" }
```
