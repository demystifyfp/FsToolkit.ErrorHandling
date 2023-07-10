# ResultOption.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Result<'b option, 'c>) -> Result<'a option, 'c> 
  -> Result<'b option, 'c>
```

## Examples

Take the following function for example

```fsharp
// string -> Result<int option, string>
let tryParseInt (s: string) =
    match Int32.TryParse(s) with
    | true, i -> Ok(Some i)
    | false, _ -> Error "Could not parse string as int"
```

### Example 1

```fsharp
let result =
    Ok(Some "123") // Result<string option, 'error>
    |> ResultOption.bind tryParseInt // Result<int option, 'error>

// Ok (Some 123)
```

### Example 2

```fsharp
let result =
    Ok None // Result<string option, 'error>
    |> ResultOption.bind tryParseInt // Result<int option, 'error>

// Ok None
```

### Example 3

```fsharp
let result =
    Ok(Some "bad things happened") // Result<string option, string>
    |> ResultOption.bind tryParseInt // Result<int option, string>

// Error "Could not parse string as int"
```

### Example 4

```fsharp
let result =
    Error "bad things happened" // Result<string option, string>
    |> ResultOption.bind tryParseInt // Result<int option, string>

// Error "bad things happened"
```
