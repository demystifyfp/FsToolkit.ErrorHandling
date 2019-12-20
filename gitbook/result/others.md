## Other Useful Functions

### requireTrue

Returns the specified error if the value is `false`.
```fsharp
'a -> bool -> Result<unit, 'a>
```
### requireFalse

Returns the specified error if the value is `true`.
```fsharp
'a -> bool -> Result<unit, 'a>
```


### requireSome

Converts an Option to a Result, using the given error if None.
```fsharp
'a -> 'b option -> Result<'b, 'a>
```
### requireNone

Converts an Option to a Result, using the given error if Some.
```fsharp
'a -> 'b option -> Result<unit, 'a>
```
### requireNotNull

Converts a nullable value to a Result, using the given error if null.
```fsharp
'a -> 'b -> Result<'b, 'a>
```


### requireEqual

Returns Ok if the two values are equal, or the specified error if not. Same as `requireEqualTo`, but with a parameter order that fits normal function application better than piping.
```fsharp
'a -> 'a -> 'b -> Result<unit, 'b>
```


### requireEqualTo

Returns Ok if the two values are equal, or the specified error if not. Same as `requireEqual`, but with a parameter order that fits piping better than normal function application.

```fsharp
'a -> 'b -> 'a  -> Result<unit, 'b>
```

### requireEmpty

Returns Ok if the sequence is empty, or the specified error if not.

```fsharp
'a -> seq<'b> -> Result<unit, 'a>
```

### requireNotEmpty

Returns the specified error if the sequence is empty, or Ok if not.

```fsharp
'a -> seq<'b> -> Result<unit, 'a>
```

### requireHead

Returns the first item of the sequence if it exists, or the specified error if the sequence is empty

```fsharp
'a -> seq<'b> -> Result<'b, 'a>
```


### setError

Replaces an error value with a custom error value

```fsharp
'a -> Result<'b, 'c> -> Result<'b, 'a>
```

### withError

Replaces a unit error value with a custom error value. Safer than `setError` since you're not losing any information.

```fsharp
'a -> Result<'b, unit> -> Result<'b, 'a>
```


### defaultValue

Returns the contained value if Ok, otherwise returns the provided value

```fsharp
'a -> Result<'a, 'b> -> 'a
```

### defaultWith

Returns the contained value if Ok, otherwise evaluates the given function and returns the result.

```fsharp
(unit -> 'a) -> Result<'a, 'b> -> 'a
```


### valueOr

Returns the Ok value or runs the specified function over the error value.

```fsharp
('b -> 'a) -> Result<'a, 'b> -> 'a
```


### ignoreError

Same as `defaultValue` for a result where the Ok value is unit. The name describes better what is actually happening in this case.

```fsharp
Result<unit, 'a> -> unit
```

### tee

If the result is Ok, executes the function on the Ok value. Passes through the input value unchanged.

```fsharp
('a -> unit) -> Result<'a, 'b> -> Result<'a, 'b>
```

### teeError

If the result is Error, executes the function on the Error value. Passes through the input value unchanged.

```fsharp
('a -> unit) -> Result<'b, 'a> -> Result<'b, 'a>
```

### teeIf

If the result is Ok and the predicate returns true for the wrapped value, executes the function on the Ok value. Passes through the input value unchanged.

```fsharp
('a -> bool) -> ('a -> unit) -> Result<'a, 'b> -> Result<'a, 'b>
```

### teeErrorIf

If the result is Error and the predicate returns true for the wrapped value, executes the function on the Error value. Passes through the input value unchanged.

```fsharp
('a -> bool) -> ('a -> unit) -> Result<'b, 'a> -> Result<'b, 'a>
```


### sequenceAsync


Converts a `Result<Async<'a>, 'b>` to `Async<Result<'a, 'b>>`.

```fsharp
Result<Async<'a>, 'b> -> Async<Result<'a, 'b>>
```
