# AsyncValidation.map

Namespace: `FsToolkit.ErrorHandling`

`map` applies a transformation to the value inside an `AsyncValidation` if it represents a successful result (`Ok`). It allows you to perform a computation on the value while preserving the success/error status of the original `AsyncValidation`. If the original `AsyncValidation` is an `Error`, `map` does nothing and returns the same `Error` unchanged.

## Function Signature

```fsharp
('okInput -> 'okOutput) -> AsyncValidation<'okInput, 'error> -> AsyncValidation<'okOutput, 'error>
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
    AsyncValidation.ok "foo" // AsyncValidation<string, string>
    |> AsyncValidation.map remainingCharacters // AsyncValidation<int, string>

// async { Ok 277 }
```

### Example 2

```fsharp
let result =
    AsyncValidation.error "bad things happened" // AsyncValidation<string, string>
    |> AsyncValidation.map remainingCharacters // AsyncValidation<int, string>

// async { Error ["bad things happened"] }
```
