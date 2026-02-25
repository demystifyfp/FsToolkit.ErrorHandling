# Other Result Functions

## isOk

Returns `true` if the value is Ok, otherwise returns `false`.

### Function Signature

```fsharp
Result<_> -> bool
```

## isError

Returns `true` if the value is Error, otherwise returns `false`.

### Function Signature

```fsharp
Result<_> -> bool
```

## sequenceAsync

Converts a `Result<Async<'a>, 'b>` to `Async<Result<'a, 'b>>`.

### Function Signature

```fsharp
Result<Async<'a>, 'b> -> Async<Result<'a, 'b>>
```

## sequenceTask

Converts a `Result<Task<'a>, 'b>` to `Task<Result<'a, 'b>>`.

### Function Signature

```fsharp
Result<Task<'a>, 'b> -> Task<Result<'a, 'b>>
```

### Examples

#### Example 1

```fsharp
let result : Task<Result<int, string>> =
    Ok (Task.FromResult 1)
    |> Result.sequenceTask
    
// Task { Ok 1 }
```

#### Example 2

```fsharp
let result : Task<Result<int, string>> =
    Error "Something went wrong"
    |> Result.sequenceTask
    
// Task { Error "Something went wrong" }
```

## traverseAsync

Converts a `Result<'a, 'error>` to `Async<Result<'b, 'error>>` by applying the given function to the Ok value.

### Function Signature

```fsharp
('okInput -> Async<'okOutput>) -> Result<'okInput, 'error> 
    -> Async<Result<'okOutput, 'error>>
```

## setError

Replaces an error value with a custom error value

### Function Signature

```fsharp
'a -> Result<'b, 'c> -> Result<'b, 'a>
```

## withError

Replaces a unit error value with a custom error value. Safer than `setError` since you're not losing any information.

### Function Signature

```fsharp
'a -> Result<'b, unit> -> Result<'b, 'a>
```

## defaultValue

Returns the contained value if Ok, otherwise returns the provided value

### Function Signature

```fsharp
'a -> Result<'a, 'b> -> 'a
```

## defaultError

Returns the contained value if Error, otherwise returns the provided value

### Function Signature

```fsharp
'b -> Result<'a, 'b> -> 'b
```

## defaultWith

Returns the contained value if Ok, otherwise evaluates the given function and returns the result.

### Function Signature

```fsharp
(unit -> 'a) -> Result<'a, 'b> -> 'a
```


## valueOr

Returns the Ok value or runs the specified function over the error value.

### Function Signature

```fsharp
('b -> 'a) -> Result<'a, 'b> -> 'a
```

## ignore

Ignores the value of the input result and returns unit instead

### Function Signature

```fsharp
Result<'ok, 'error> -> Result<unit, 'error>
```

## ignoreError

Same as `defaultValue` for a result where the Ok value is unit. The name describes better what is actually happening in this case.

### Function Signature

```fsharp
Result<unit, 'a> -> unit
```
