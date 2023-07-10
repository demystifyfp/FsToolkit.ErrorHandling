# Tee Functions

These functions allow us to execute side effects based on our result. They are useful for logging and other side effects that we want to execute without changing the result.

Consider the following code for the examples below

```fsharp
// string -> unit
let log (message: string) =
    printfn "%s" message
```

## tee

If the result is Ok, executes the function on the Ok value. Passes through the input value unchanged.

### Function Signature

```fsharp
('a -> unit) -> Result<'a, 'b> -> Result<'a, 'b>
```

### Examples

#### Example 1

```fsharp
let result : Result<int, string> =
    Ok 1
    |> Result.tee (fun value -> log (sprintf "Value is %s" value))
    
// Value is 1
// Ok 1
```

## teeError

If the result is Error, executes the function on the Error value. Passes through the input value unchanged.

### Function Signature

```fsharp
('a -> unit) -> Result<'b, 'a> -> Result<'b, 'a>
```

### Examples

#### Example 1

```fsharp
let result : Result<int, string> =
    Error "Something bad happened"
    |> Result.teeError (fun error -> log (sprintf "Error: %s" error))

// Error: Something bad happened
// Error "Something bad happened"
```

## teeIf

If the result is Ok and the predicate returns true for the wrapped value, executes the function on the Ok value. Passes through the input value unchanged.

### Function Signature

```fsharp
('a -> bool) -> ('a -> unit) -> Result<'a, 'b> -> Result<'a, 'b>
```

### Examples

#### Example 1

Since predicate condition is met, the log function is executed.

```fsharp
let result : Result<int, string> =
    Ok 1
    |> Result.teeIf (fun value -> value = 1) (fun value -> log (sprintf "Value is %s" value))
    
// Value is 1
// Ok 1
```

#### Example 2

Since predicate condition is not met, the log function is not executed.

```fsharp
let result : Result<int, string> =
    Ok 2
    |> Result.teeIf (fun value -> value = 1) (fun value -> log (sprintf "Value is %s" value))
    
// Ok 1
```

## teeErrorIf

If the result is Error and the predicate returns true for the wrapped value, executes the function on the Error value. Passes through the input value unchanged.

### Function Signature

```fsharp
('a -> bool) -> ('a -> unit) -> Result<'b, 'a> -> Result<'b, 'a>
```

### Examples

#### Example 1

Since predicate condition is met, the log function is executed.

```fsharp
let result : Result<int, string> =
    Error "Something bad happened"
    |> Result.teeErrorIf (fun error -> error = "Something bad happened") (fun error -> log (sprintf "Error: %s" error))

// Error: Something bad happened
// Error "Something bad happened"
```

#### Example 2

Since predicate condition is not met, the log function is not executed.

```fsharp
let result : Result<int, string> =
    Error "Something bad happened"
    |> Result.teeErrorIf (fun error -> error = "Something else bad happened") (fun error -> log (sprintf "Error: %s" error))

// Error "Something bad happened"
```
