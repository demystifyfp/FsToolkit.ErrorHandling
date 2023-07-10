# Either Functions

Consider the following code for the examples below

```fsharp
// int -> int
let okF (x : int) = x + 1

// int -> int
let errorF (x : int) = x - 1
```

## Result.either

Namespace: `FsToolkit.ErrorHandling`

If the result is ok, perform a function on the ok value which returns a value. If the result is an error, perform a function on the error value which returns a value.

### Function Signature

```fsharp
('okInput -> 'output) -> ('errorInput -> 'output) 
    -> Result<'okInput, 'errorInput> -> 'output
```

### Examples

#### Example 1

```fsharp
let result =
    Ok 1
    |> Result.either okF errorF
    
// 2
```

## Result.eitherMap

Namespace: `FsToolkit.ErrorHandling`

If the result is ok, perform a function on the ok value to map it to another result type. If the result is an error, perform a function on the error value to map it to another result type.

### Function Signature

```fsharp
('okInput -> 'okOutput) -> ('errorInput -> 'errorOutput) -> Result<'okInput, 'errorInput> 
    -> Result<'okOutput, 'errorOutput>
```

### Examples

#### Example 1

```fsharp
let result =
    Error 1
    |> Result.eitherMap okF errorF
    
// 0
```
