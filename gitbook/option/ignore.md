# Option.ignore

Namespace: `FsToolkit.ErrorHandling`

`ignore` ignores the value inside an option and returns a `unit option`. Returns `Some ()` if the option is `Some`, or `None` if it is `None`.

We can call this with the `do!` syntax inside a computation expression using `Option.ignore` as below.
Note the type being ignored must be specified to prevent accidental variances in return type deviating from the code's intent.

## Function Signature

```fsharp
'T option -> unit option
```

## Examples

### Example 1

```fsharp
let result : unit option =
    Some 42
    |> Option.ignore<int>

// Some ()
```

### Example 2

```fsharp
let result : unit option =
    Some "hello"
    |> Option.ignore<string>

// Some ()
```

### Example 3

```fsharp
let result : unit option =
    None
    |> Option.ignore<_>

// None
```

### Example 4

```fsharp
let result = result {
    // do! Some "a" |> Option.ignore<int> // WOULD NOT COMPILE
    do! Some 21 |> Option.ignore<int> // Compiles, as an Option<unit> is equivalent to a function returning unit
    return 42
}

// Some 42
```

