# AsyncValidation.mapError

Namespace: `FsToolkit.ErrorHandling`

`mapError` takes an async validation and a normal function and returns a new async validation value based on the input error value and the function

## Function Signature

```fsharp
('errorInput -> 'errorOutput) -> AsyncValidation<'ok, 'errorInput> 
    -> AsyncValidation<'ok, 'errorOutput>
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
    AsyncValidation.ok "all good" // AsyncValidation<string, string>
    |> AsyncValidation.mapError getErrorCode // AsyncValidation<string, int>

// async { Ok "all good" }
```

### Example 2

```fsharp
let result =
    AsyncValidation.error "bad things happened" // AsyncValidation<string, string>
    |> AsyncValidation.mapError getErrorCode // AsyncValidation<string, int>

// async { Error [1] }
```
