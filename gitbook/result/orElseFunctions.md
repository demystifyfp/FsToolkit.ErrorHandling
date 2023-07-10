# OrElse Functions

## Result.orElse

Namespace: `FsToolkit.ErrorHandling`

Returns the result if the result is Ok, otherwise returns the given result

### Function Signature

```fsharp
Result<'ok, 'errorOutput> -> Result<'ok, 'error> 
    -> Result<'ok, 'errorOutput>
```

### Examples

#### Example 1

```fsharp
let result : Result<int, string> =
    Ok 1
    |> Result.orElse (Ok 2)
    
// Ok 1
```

#### Example 2

```fsharp
let result : Result<int, string> =
    Ok 1
    |> Result.orElse (Error "Error")
    
// Ok 1
```

#### Example 3

```fsharp
let result : Result<int, string> =
    Error "Error"
    |> Result.orElse (Ok 2)
    
// Ok 2
```

#### Example 4

```fsharp
let result : Result<int, string> =
    Error "Error"
    |> Result.orElse (Error "Error 2")

// Error "Error 2"
```

## Result.orElseWith

Namespace: `FsToolkit.ErrorHandling`

Returns the result if the result is Ok, otherwise evaluates the given function and returns the result.

### Function Signature

```fsharp
('error -> Result<'ok, 'errorOutput>) -> Result<'ok, 'error> 
    -> Result<'ok, 'errorOutput>
```

### Examples

#### Example 1

```fsharp
let result : Result<int, int> =
    Ok 1
    |> Result.orElseWith (fun e -> e + 1 |> Ok)

// Ok 1
```

#### Example 2

```fsharp
let result : Result<int, int> =
    Ok 1
    |> Result.orElseWith (fun e -> e + 1 |> Error)

// Ok 1
```

#### Example 3

```fsharp
let result : Result<int, int> =
    Error 1
    |> Result.orElseWith (fun e -> e + 1 |> Ok)

// Ok 2
```

#### Example 4

```fsharp
let result : Result<int, int> =
    Error 1
    |> Result.orElseWith (fun e -> e + 1 |> Error)

// Error 2
```
