# Result.bind

Namespace: `FsToolkit.ErrorHandling`

`bind` takes a transformation function `'okInput -> Result<'okOutput, 'error>` and a `Result<'okInput, 'error>`. If the Result is `Ok x`, it applies the transformation function to `x`, which returns a new `Result<'okOutput, 'error>`. The bind function then returns the new `Result<'okOutput, 'error>`. If the original Result is an Error, it simply returns the original Error unchanged without invoking the transformation function.

## Function Signature

```fsharp
('okInput -> Result<'okOutput, 'error>) -> Result<'okInput, 'error> 
    -> Result<'okOutput, 'error>
```

## Examples

Take the following function for example

```fsharp
// string -> Result<int, string>
let tryParseInt (s: string) =
    match Int32.TryParse(s) with
    | true, i -> Ok i
    | false, _ -> Error "Could not parse string as int"
```

### Example 1

```fsharp
let result =
    Ok "123" // Result<string, string>
    |> ResultOption.bind tryParseInt // Result<int, string>

// Ok 123
```

### Example 2

```fsharp
let result =
    Ok "bad things happened" // Result<string, string>
    |> ResultOption.bind tryParseInt // Result<int, string>

// Error "Could not parse string as int"
```

### Example 3

```fsharp
let result =
    Error "bad things happened" // Result<string, string>
    |> ResultOption.bind tryParseInt // Result<int, string>

// Error "bad things happened"
```
