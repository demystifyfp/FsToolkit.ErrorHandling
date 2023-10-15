# Option.bind

Namespace: `FsToolkit.ErrorHandling`

## Function Signature

```fsharp
('TInput -> 'TOutput option) -> 'TInput option -> 'TOutput option
```

## Examples

Take the following function for example

```fsharp
// string -> int option
let tryParseInt (s: string) =
    match Int32.TryParse(s) with
    | true, i -> Some i
    | false, _ -> None
```

### Example 1

```fsharp
let opt : int option =
    Some "123" // string option
    |> Option.bind tryParseInt // int option

// Some 123
```

### Example 2

```fsharp
let opt : int option =
    Some "Not a number" // string option
    |> Option.bind tryParseInt // int option

// None
```

### Example 3

```fsharp
let opt : int option =
    None // string option
    |> Option.bind tryParseInt // int option

// None
```
