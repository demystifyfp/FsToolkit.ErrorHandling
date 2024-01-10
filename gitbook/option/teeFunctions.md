# Tee Functions

These functions allow us to execute side effects based on our option. They are useful for logging and other side effects that we want to execute without changing the option.

Consider the following code for the examples below

```fsharp
// string -> unit
let log (message: string) =
    printfn "%s" message
```

## teeSome

If the option is Some, executes the function on the Some value. Passes through the input value unchanged.

### Function Signature

```fsharp
('a -> unit) -> 'a option -> 'a option
```

### Examples

#### Example 1

```fsharp
let option : int option =
    Some 1
    |> Option.teeSome (fun value -> log (sprintf "Value is %i" value))

// Value is 1
// Some 1
```

## teeNone

If the option is None, executes the function. Passes through the input value unchanged.

### Function Signature

```fsharp
(unit -> unit) -> 'a option -> 'a option
```

### Examples

#### Example 1

```fsharp
let option : int option =
    None
    |> Option.teeNone (fun () -> log "Option is None")

// Option is None
// None
```

## teeIf

If the option is Some and the predicate returns true for the wrapped value, executes the function on the Some value. Passes through the input value unchanged.

### Function Signature

```fsharp
('a -> bool) -> ('a -> unit) -> 'a option -> 'a option
```

### Examples

#### Example 1

```fsharp
let option : int option =
    Some 1
    |> Option.teeIf (fun value -> value = 1) (fun value -> log (sprintf "Value is %i" value))

// Value is 1
// Some 1
```

#### Example 2

```fsharp
let option : int option =
    Some 1
    |> Option.teeIf (fun value -> value = 2) (fun value -> log (sprintf "Value is %i" value))

// Some 1
```
