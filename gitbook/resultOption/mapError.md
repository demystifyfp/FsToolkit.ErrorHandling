## ResultOption.mapError

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('errorInput -> 'errorOutput) -> Result<'ok option, 'errorInput> -> Result<'ok option, 'errorOutput>
```

## Examples

### Example 1

From the [ResultOption.map](../resultOption/map.md#example-3) example, if we want to map the error part alone, we can do it as below:

```fsharp
// string -> int
let getErrorCode (message: string) =
    match message with
    | "bad things happened" -> 1
    | _ -> 0

let result : Result<string option, int> =
  Error "bad things happened" // Result<string option, string>
  |> ResultOption.mapError getErrorCode // Result<string option, int>

// Error 1
```

