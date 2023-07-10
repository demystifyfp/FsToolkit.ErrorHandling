# ResultOption.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Result<('a -> 'b) option, 'c> -> Result<'a option, 'c> 
  -> Result<'b option, 'c>
```

## Examples

Take the following function for example

```fsharp
// string -> int
let characterCount (s: string) = s.Length
```

### Example 1

```fsharp
let result =
    Ok(Some "foo") // Result<string option, 'error>
    |> ResultOption.apply (Ok(Some characterCount)) // Result<int option, 'error>

// Ok (Some 3)
```

### Example 2

```fsharp
let result =
    Ok None // Result<string option, 'error>
    |> ResultOption.apply (Ok(Some characterCount)) // Result<int option, 'error>

// Ok None
```

### Example 3

```fsharp
let result =
    Error "bad things happened" // Result<string option, string>
    |> ResultOption.apply (Ok(Some characterCount)) // Result<int option, string>

// Error "bad things happened"
```
