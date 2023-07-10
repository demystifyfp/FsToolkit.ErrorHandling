# ResultOption.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> Result<'a option, 'c> -> Result<'b option, 'c>
```

`ResultOption.map` is the same as `Result.map Option.map`.

## Examples

Take the following functions for example

```fsharp
// string -> int
let remainingCharacters (string: prompt) =
    280 - prompt.Length
```

### Example 1

```fsharp
let result =
    Ok(Some "foo") // Result<string option, 'error>
    |> ResultOption.map remainingCharacters // Result<int option, 'error>

// Ok (Some 277)
```

### Example 2

```fsharp
let result =
    Ok None // Result<string option, 'error>
    |> ResultOption.map remainingCharacters // Result<int option, 'error>

// Ok None
```

### Example 3

```fsharp
let result =
    Error "bad things happened" // Result<string option, string>
    |> ResultOption.map remainingCharacters // Result<int option, string>

// Error "bad things happened"
```
