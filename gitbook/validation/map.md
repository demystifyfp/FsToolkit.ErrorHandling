# Validation.map

Namespace: `FsToolkit.ErrorHandling`

`map` applies a transformation to the value inside a `Validation` if it represents a successful result (`Ok`). It allows you to perform a computation on the value while preserving the success/error status of the original `Validation`. If the original `Validation` is an `Error`, `map` does nothing and returns the same `Error` unchanged.

## Function Signature

```fsharp
('okInput -> 'okOutput) -> Validation<'okInput, 'error> -> Validation<'okOutput, 'error>
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
    Validation.ok "foo" // Validation<string, string>
    |> Validation.map remainingCharacters // Validation<int, string>

// Ok 277
```

### Example 2

```fsharp
let result =
    Validation.error "bad things happened" // Validation<string, string>
    |> Validation.map remainingCharacters // Validation<int, string>

// Error ["bad things happened"]
```
