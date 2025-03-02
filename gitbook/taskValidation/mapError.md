# TaskValidation.mapError

Namespace: `FsToolkit.ErrorHandling`

`mapError` takes an task validation and a normal function and returns a new task validation value based on the input error value and the function

## Function Signature

```fsharp
('errorInput -> 'errorOutput) -> TaskValidation<'ok, 'errorInput> 
    -> TaskValidation<'ok, 'errorOutput>
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
    TaskValidation.ok "all good" // TaskValidation<string, string>
    |> TaskValidation.mapError getErrorCode // TaskValidation<string, int>

// task { Ok "all good" }
```

### Example 2

```fsharp
let result =
    TaskValidation.error "bad things happened" // TaskValidation<string, string>
    |> TaskValidation.mapError getErrorCode // TaskValidation<string, int>

// task { Error [1] }
```
