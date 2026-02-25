# Option.ignore

Namespace: `FsToolkit.ErrorHandling`

`ignore` ignores the value inside an option and returns a `unit option`. Returns `Some ()` if the option is `Some`, or `None` if it is `None`.

## Function Signature

```fsharp
'T option -> unit option
```

## Examples

### Example 1

```fsharp
let result : unit option =
    Some 42
    |> Option.ignore

// Some ()
```

### Example 2

```fsharp
let result : unit option =
    Some "hello"
    |> Option.ignore

// Some ()
```

### Example 3

```fsharp
let result : unit option =
    None
    |> Option.ignore

// None
```
