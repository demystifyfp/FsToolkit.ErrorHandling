# Result.mapError

Namespace: `FsToolkit.ErrorHandling`

`mapError` takes a result and a normal function and returns a new result value based on the input error value and the function

## Function Signature

```fsharp
('errorInput -> 'errorOutput) -> Result<'ok, 'errorInput> 
    -> Result<'ok, 'errorOutput>
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
    Ok "all good" // Result<string, string>
    |> Result.mapError getErrorCode // Result<string, int>

// Ok "all good"
```

### Example 2

```fsharp
let result =
    Error "bad things happened" // Result<string, string>
    |> Result.mapError getErrorCode // Result<string, int>

// Error 1
```
