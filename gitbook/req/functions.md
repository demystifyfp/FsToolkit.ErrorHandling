# Req Functions

## filter

### Function Signature

Applies a predicate to the supplied value.
If the predicate returns `true`, then returns the value wrapped in `Ok`.
Otherwise, returns an `Error` result with the provided `error`.

```fsharp
('input -> bool) -> 'error -> 'input -> Result<'input,'error>
```

#### Example 1

```fsharp
let result: Result<string, string> = 
    "F#"
    |> Req.filter _.Contains("#") "Provided input does not contain #"

// Ok "F#"
```

#### Example 2

```fsharp
let result: Result<string, string> = 
    "Hello World!"
    |> Req.filter _.Contains("#") "Provided input does not contain #"

// Error "Provided input does not contain #"
```

## isTrue

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
    |> Req.isTrue "Value must be true"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    false
    |> Req.isTrue "Value must be true"
    
// Error "Value must be true"
```

## isFalse

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
    |> Req.isFalse "Value must be false"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    true
    |> Req.isFalse "Value must be false"
    
// Error "Value must be false"
```

## some

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
    |> Req.some "Value must be Some"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    None
    |> Req.some "Value must be Some"
    
// Error "Value must be Some"
```

## none

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
    |> Req.none "Value must be None"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    Some 1
    |> Result.requireNone "Value must be None"
    
// Error "Value must be None"
```

## valueSome

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
    |> Req.valueSome "Value must be ValueSome"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    ValueNone
    |> Req.valueSome "Value must be ValueSome"
    
// Error "Value must be ValueSome"
```

## valueNone

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
    |> Req.valueNone "Value must be ValueNone"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    ValueSome 1
    |> Req.valueNone "Value must be ValueNone"
    
// Error "Value must be ValueNone"
```

## notNull

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
    |> Req.notNull "Value must be not null"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    null
    |> Req.notNull "Value must be not null"
    
// Error "Value must be not null"
```

## equal

Returns Ok if the two values are equal, or the specified error if not. Same as `Req.equalTo`, but with a parameter order that fits normal function application better than piping.

### Function Signature

```fsharp
'b -> 'a * 'a -> Result<unit, 'b>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    (1, 1) |> Req.equal "Value must be equal to 1"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    (1, 2) |> Req.equal "Value must be equal to 1"
    
// Error "Value must be equal to 1"
```

## equalTo

Returns Ok if the two values are equal, or the specified error if not. Same as `Req.equal`, but with a parameter order that fits piping better than normal function application.

### Function Signature

```fsharp
'a -> 'b -> 'a  -> Result<unit, 'b>
```

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    1
    |> Req.equalTo 1 "Value must be equal to 1"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    2
    |> Req.equalTo 1 "Value must be equal to 1"
    
// Error "Value must be equal to 1"
```

## empty

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
    |> Req.empty "Value must be empty"

// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    [1]
    |> Req.empty "Value must be empty"
    
// Error "Value must be empty"
```

## notEmpty

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
    |> Req.notEmpty "Value must not be empty"
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    []
    |> Req.notEmpty "Value must not be empty"
    
// Error "Value must not be empty"
```

## head

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
    |> Req.head "Seq must have head"

// Ok 1
```

#### Example 2

```fsharp
let result : Result<int, string> =
    []
    |> Req.head "Seq must have head"

// Error "Seq must have head"
```

## with

A `*With` function has identical behavior to the associated underlying function, except that the error
becomes a function that is evaluated only if the underlying function has determined the check has failed.

This allows expensive or side-effecting error construction to be avoided unless it is actually needed.

### Examples

#### Example 1

```fsharp
let result : Result<unit, string> =
    true
    |> Req.isTrue "Value must be true"
    
// Ok ()
```

```fsharp
let result : Result<unit, string> =
    true
    |> Req.isTrueWith (fun () -> printf "failed"; invalidOp "Value must be true")
    
// Ok ()
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    false
    |> Req.isTrueWith (fun () -> translate "Value must be true")
    
// Error "De waarde moet waar zijn"
```

#### Example 2

```fsharp
let result : Result<unit, string> =
    false
    |> Req.isTrueWith (fun () -> $"Value must be true. The answer is... {41 + 1}")
    
// Error "Value must be true. The answer is... 42"
```

#### someWith

Converts an Option to a Result, using the given error factory if None. The error factory is only called when the value is `None`.

##### Function Signature

```fsharp
(unit -> 'a) -> 'b option -> Result<'b, 'a>
```

##### Examples

###### Example 1

```fsharp
let result : Result<int, string> =
    Some 1
    |> Req.someWith (fun () -> "Value must be Some")
    
// Ok 1
```

###### Example 2

```fsharp
let result : Result<int, string> =
    None
    |> Req.someWith (fun () -> "Value must be Some")
    
// Error "Value must be Some"
```

#### noneWith

Converts an Option to a Result, using the given error factory if Some. The error factory is only called when the value is `Some`.

##### Function Signature

```fsharp
(unit -> 'a) -> 'b option -> Result<unit, 'a>
```

##### Examples

###### Example 1

```fsharp
let result : Result<unit, string> =
    None
    |> Req.noneWith (fun () -> "Value must be None")
    
// Ok ()
```

###### Example 2

```fsharp
let result : Result<unit, string> =
    Some 1
    |> Req.noneWith (fun () -> "Value must be None")
    
// Error "Value must be None"
```

