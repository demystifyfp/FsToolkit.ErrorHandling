# AsyncValidation.mapErrors

Namespace: `FsToolkit.ErrorHandling`

Similar to [AsyncValidation.mapError](../asyncValidation/mapError.md), except that the mapping function is passed the full list of errors, rather than each one individually.

## Function Signature

```fsharp
('errorInput list -> 'errorOutput list) -> AsyncValidation<'ok, 'errorInput> 
    -> AsyncValidation<'ok, 'errorOutput>
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
    AsyncValidation.ok "all good" // AsyncValidation<string, string>
    |> AsyncValidation.mapErrors getErrorCode // AsyncValidation<string, int>

// async { Ok "all good" }
```

### Example 2

```fsharp
let result : AsyncValidation<string, int> =
    AsyncValidation.error "bad things happened" // AsyncValidation<string, string>
    |> AsyncValidation.mapErrors getErrorCode // AsyncValidation<string, int>

// async { Error [1] }
```
