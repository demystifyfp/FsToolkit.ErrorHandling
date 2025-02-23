# Validation.mapError

Namespace: `FsToolkit.ErrorHandling`

`mapError` takes a validation and a normal function and returns a new validation value based on the input error value and the function

## Function Signature

```fsharp
('errorInput -> 'errorOutput) -> Validation<'ok, 'errorInput> 
    -> Validation<'ok, 'errorOutput>
```

## Examples

Take the following functions for example

```fsharp
// string -> int
let getErrorCode (message: string) =
    match message with
    | "bad things happened" -> 1
    | _ -> 0
```

### Example 1

```fsharp
let result =
    Validation.ok "all good" // Validation<string, string>
    |> Validation.mapError getErrorCode // Validation<string, int>

// Ok "all good"
```

### Example 2

```fsharp
let result =
    Validation.error "bad things happened" // Validation<string, string>
    |> Validation.mapError getErrorCode // Validation<string, int>

// Error [1]
```
