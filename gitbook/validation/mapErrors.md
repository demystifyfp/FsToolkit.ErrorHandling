# Validation.mapErrors

Namespace: `FsToolkit.ErrorHandling`

Similar to [Validation.mapError](../validation/mapError.md), except that the mapping function is passed the full list of errors, rather than each one individually.

## Function Signature

```fsharp
('errorInput list -> 'errorOutput list) -> Validation<'ok, 'errorInput> 
    -> Validation<'ok, 'errorOutput>
```

## Examples

Take the following functions for example

```fsharp
// string -> int
let getErrorCode (messages: string list) =
    match messages |> List.tryFind ((=) "bad things happened") with
    | Some _ -> [1]
    | _ -> [0]
```

### Example 1

```fsharp
let result =
    Validation.ok "all good" // Validation<string, string>
    |> Validation.mapErrors getErrorCode // Validation<string, int>

// Ok "all good"
```

### Example 2

```fsharp
let result : Validation<string, int> =
    Validation.error "bad things happened" // Validation<string, string>
    |> Validation.mapErrors getErrorCode // Validation<string, int>

// Error [1]
```
