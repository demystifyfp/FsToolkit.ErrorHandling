# TaskValidation.mapErrors

Namespace: `FsToolkit.ErrorHandling`

Similar to [TaskValidation.mapError](../taskValidation/mapError.md), except that the mapping function is passed the full list of errors, rather than each one individually.

## Function Signature

```fsharp
('errorInput list -> 'errorOutput list) -> TaskValidation<'ok, 'errorInput> 
    -> TaskValidation<'ok, 'errorOutput>
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
    TaskValidation.ok "all good" // TaskValidation<string, string>
    |> TaskValidation.mapErrors getErrorCode // TaskValidation<string, int>

// task { Ok "all good" }
```

### Example 2

```fsharp
let result : TaskValidation<string, int> =
    TaskValidation.error "bad things happened" // TaskValidation<string, string>
    |> TaskValidation.mapErrors getErrorCode // TaskValidation<string, int>

// task { Error [1] }
```
