# Result.map

Namespace: `FsToolkit.ErrorHandling`

`map` applies a transformation to the value inside a `Result` if it represents a successful result (`Ok`). It allows you to perform a computation on the value while preserving the success/error status of the original `Result`. If the original `Result` is an `Error`, `map` does nothing and returns the same `Error` unchanged.

## Function Signature

```fsharp
('okInput -> 'okOutput) -> Result<'okInput, 'error> -> Result<'okOutput, 'error>
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
let result =
    Ok "foo" // Result<string, string>
    |> Result.map remainingCharacters // Result<int, string>

// Ok 277
```

### Example 2

```fsharp
let result =
    Error "bad things happened" // Result<string, string>
    |> Result.map remainingCharacters // Result<int, string>

// Error "bad things happened"
```
