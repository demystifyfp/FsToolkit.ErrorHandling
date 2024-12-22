# Require Functions

## requireTrue

Returns the specified error if the value is `false`.

### Function Signature

```fsharp
'a -> bool -> Result<unit, 'a>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    true
    |> Result.requireTrue "Value must be true"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    false
    |> Result.requireTrue "Value must be true"
    
// Error "Value must be true"
```

## requireFalse

Returns the specified error if the value is `true`.

### Function Signature

```fsharp
'a -> bool -> Result<unit, 'a>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    false
    |> Result.requireFalse "Value must be false"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    true
    |> Result.requireFalse "Value must be false"
    
// Error "Value must be false"
```

## requireSome

Converts an Option to a Result, using the given error if None.

### Function Signature

```fsharp
'a -> 'b option -> Result<'b, 'a>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    Some 1
    |> Result.requireSome "Value must be Some"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    None
    |> Result.requireSome "Value must be Some"
    
// Error "Value must be Some"
```

## requireNone

Converts an Option to a Result, using the given error if Some.

### Function Signature

```fsharp
'a -> 'b option -> Result<unit, 'a>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    None
    |> Result.requireNone "Value must be None"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    Some 1
    |> Result.requireNone "Value must be None"
    
// Error "Value must be None"
```

## requireValueSome

Converts an ValueOption to a Result, using the given error if ValueNone.

### Function Signature

```fsharp
'a -> 'b voption -> Result<'b, 'a>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    ValueSome 1
    |> Result.requireValueSome "Value must be ValueSome"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    None
    |> Result.requireValueSome "Value must be ValueSome"
    
// Error "Value must be ValueSome"
```

## requireValueNone

Converts an ValueOption to a Result, using the given error if ValueSome.

### Function Signature

```fsharp
'a -> 'b voption -> Result<unit, 'a>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    ValueNone
    |> Result.requireValueNone "Value must be ValueNone"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    ValueSome 1
    |> Result.requireValueNone "Value must be ValueNone"
    
// Error "Value must be ValueNone"
```

## requireNotNull

Converts a nullable value to a Result, using the given error if null.

### Function Signature

```fsharp
'a -> 'b -> Result<'b, 'a>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    1
    |> Result.requireNotNull "Value must be not null"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    null
    |> Result.requireNotNull "Value must be not null"
    
// Error "Value must be not null"
```

## requireEqual

Returns Ok if the two values are equal, or the specified error if not. Same as `requireEqualTo`, but with a parameter order that fits normal function application better than piping.

### Function Signature

```fsharp
'a -> 'a -> 'b -> Result<unit, 'b>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    Result.requireEqual 1 1 "Value must be equal to 1"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    Result.requireEqual 1 2 "Value must be equal to 1"
    
// Error "Value must be equal to 1"
```

## requireEqualTo

Returns Ok if the two values are equal, or the specified error if not. Same as `requireEqual`, but with a parameter order that fits piping better than normal function application.

### Function Signature

```fsharp
'a -> 'b -> 'a  -> Result<unit, 'b>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    1
    |> Result.requireEqualTo "Value must be equal to 1" 1
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    2
    |> Result.requireEqualTo "Value must be equal to 1" 1
    
// Error "Value must be equal to 1"
```

## requireEmpty

Returns Ok if the sequence is empty, or the specified error if not.

### Function Signature

```fsharp
'a -> seq<'b> -> Result<unit, 'a>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    []
    |> Result.requireEmpty "Value must be empty"

// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    [1]
    |> Result.requireEmpty "Value must be empty"
    
// Error "Value must be empty"
```

## requireNotEmpty

Returns the specified error if the sequence is empty, or Ok if not.

### Function Signature

```fsharp
'a -> seq<'b> -> Result<unit, 'a>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    [1]
    |> Result.requireNotEmpty "Value must not be empty"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    []
    |> Result.requireNotEmpty "Value must not be empty"
    
// Error "Value must not be empty"
```

## requireHead

Returns the first item of the sequence if it exists, or the specified error if the sequence is empty

### Function Signature

```fsharp
'a -> seq<'b> -> Result<'b, 'a>
```

### Examples

#### Example 1

```fsharp
let result : Result<int, string> =
    [1; 2; 3]
    |> Result.requireHead "Seq must have head"

// Ok 1
```

#### Example 2

```fsharp
let result : Result<int, string> =
    []
    |> Result.requireHead "Seq must have head"

// Error "Seq must have head"
```

## require

### Function Signature

If the input result is `Ok`, applies a predicate to the `Ok` value.
If the predicate returns true, then returns the original `Ok` Result.
Otherwise, returns a new `Error` result with the provided error.

```fsharp
('ok -> bool) -> 'error -> Result<'ok,'error> -> Result<'ok,'error>
```
Note: 
If you find that you need the Ok value to produce an appropriate error, use the `check` method instead.

#### Example 1

```fsharp
let result: Result<string, string> = 
    Result.Ok "F#"
    |> Result.require 
        (_.Contains("#")) 
        "Provided input does not contain #"

// Ok "F#"
```

#### Example 2

```fsharp
let result: Result<string, string> = 
    Result.Ok "Hello World!"
    |> Result.require 
        (_.Contains("#")) 
        "Provided input does not contain #"

// Error "Provided input does not contain #"
```


