# Option.tryParse

Namespace: `FsToolkit.ErrorHandling`

`tryParse` tries to parse a string value into a specified type using the type's `TryParse` method. Returns `Some value` if parsing succeeds, or `None` if it fails.

> **Note:** This function is not available when using Fable (JavaScript/Python compilation).

## Function Signature

```fsharp
string -> ^value option
```

where `^value` has a static member `TryParse: string * byref<^value> -> bool`.

## Examples

### Example 1 — Parsing an integer

```fsharp
let result : int option =
    Option.tryParse "42"

// Some 42
```

### Example 2 — Parsing failure

```fsharp
let result : int option =
    Option.tryParse "not a number"

// None
```

### Example 3 — Parsing a boolean

```fsharp
let result : bool option =
    Option.tryParse "true"

// Some true
```

### Example 4 — Parsing a DateTime

```fsharp
let result : System.DateTime option =
    Option.tryParse "2024-01-15"

// Some 2024-01-15T00:00:00
```
