# TaskValidation.map

Namespace: `FsToolkit.ErrorHandling`

`map` applies a transformation to the value inside an `TaskValidation` if it represents a successful result (`Ok`). It allows you to perform a computation on the value while preserving the success/error status of the original `TaskValidation`. If the original `TaskValidation` is an `Error`, `map` does nothing and returns the same `Error` unchanged.

## Function Signature

```fsharp
('okInput -> 'okOutput) -> TaskValidation<'okInput, 'error> -> TaskValidation<'okOutput, 'error>
```

## Examples

Take the following functions for example

```fsharp
// string -> int
let remainingCharacters (prompt: string) =
    280 - prompt.Length
```

### Example 1

```fsharp
let validation =
    TaskValidation.ok "foo" // TaskValidation<string, string>
    |> TaskValidation.map remainingCharacters // TaskValidation<int, string>

// task { Ok 277 }
```

### Example 2

```fsharp
let result =
    TaskValidation.error "bad things happened" // TaskValidation<string, string>
    |> TaskValidation.map remainingCharacters // TaskValidation<int, string>

// task { Error ["bad things happened"] }
```
