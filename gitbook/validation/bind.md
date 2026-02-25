# Validation.bind

Namespace: `FsToolkit.ErrorHandling`

`bind` applies a function to the value inside a `Validation` if it is `Ok`, returning the resulting `Validation`. Unlike `apply` and `map2`/`map3`, `bind` uses **monadic (short-circuit) semantics** — if the input is an `Error`, the binder function is not called and the error is returned immediately.

## Function Signature

```fsharp
('okInput -> Validation<'okOutput, 'error>) -> Validation<'okInput, 'error> -> Validation<'okOutput, 'error>
```

## Examples

Take the following function for example:

```fsharp
// string -> Validation<int, string>
let tryParseInt (s: string) =
    match System.Int32.TryParse(s) with
    | true, i -> Validation.ok i
    | false, _ -> Validation.error $"'%s{s}' is not a valid integer"
```

### Example 1

```fsharp
let result =
    Validation.ok "42"
    |> Validation.bind tryParseInt

// Ok 42
```

### Example 2

```fsharp
let result =
    Validation.ok "not a number"
    |> Validation.bind tryParseInt

// Error ["'not a number' is not a valid integer"]
```

### Example 3

```fsharp
let result =
    Validation.error "previous error"
    |> Validation.bind tryParseInt

// Error ["previous error"]
```
