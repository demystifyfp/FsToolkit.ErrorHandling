## Other Useful Functions

### requireTrue

Returns the specified error if the value is `false`.
```F#
'a -> bool -> Result<unit, 'a>
```
### requireFalse

Returns the specified error if the value is `true`.
```F#
'a -> bool -> Result<unit, 'a>
```


### requireSome

Converts an Option to a Result, using the given error if None.
```F#
'a -> 'b option -> Result<'b, 'a>
```
### requireNone

Converts an Option to a Result, using the given error if Some.
```F#
'a -> 'b option -> Result<unit, 'a>
```

### requireEqual

Returns Ok if the two values are equal, or the specified error if not. Same as `requireEqualTo`, but with a parameter order that fits normal function application better than piping.
```F#
'a -> 'a -> 'b -> Result<unit, 'b>
```


### requireEqualTo

Returns Ok if the two values are equal, or the specified error if not. Same as `requireEqual`, but with a parameter order that fits piping better than normal function application.

```F#
'a -> 'b -> 'a  -> Result<unit, 'b>
```

### requireEmpty

Returns Ok if the sequence is empty, or the specified error if not.

```F#
'a -> seq<'b> -> Result<unit, 'a>
```

### requireNotEmpty

Returns the specified error if the sequence is empty, or Ok if not.

```F#
'a -> seq<'b> -> Result<unit, 'a>
```

### requireHead

Returns the first item of the sequence if it exists, or the specified error if the sequence is empty

```F#
'a -> seq<'b> -> Result<'b, 'a>
```


### setError

Replaces an error value with a custom error value

```F#
'a -> Result<'b, 'c> -> Result<'b, 'a>
```

### withError

Replaces a unit error value with a custom error value. Safer than `setError` since you're not losing any information.

```F#
'a -> Result<'b, unit> -> Result<'b, 'a>
```


### defaultValue

Returns the contained value if Ok, otherwise returns the provided value

```F#
'a -> Result<'a, 'b> -> 'a
```

### defaultWith

Returns the contained value if Ok, otherwise evaluates the given function and returns the result.

```F#
(unit -> 'a) -> Result<'a, 'b> -> 'a
```


### ignoreError

Same as `defaultValue` for a result where the Ok value is unit. The name describes better what is actually happening in this case.

```F#
Result<unit, 'a> -> unit
```

### tee

If the result is Ok, executes the function on the Ok value. Passes through the input value unchanged.

```F#
('a -> unit) -> Result<'a, 'b> -> Result<'a, 'b>
```

### teeError

If the result is Error, executes the function on the Error value. Passes through the input value unchanged.

```F#
('a -> unit) -> Result<'b, 'a> -> Result<'b, 'a>
```

### teeIf

If the result is Ok and the predicate returns true for the wrapped value, executes the function on the Ok value. Passes through the input value unchanged.

```F#
('a -> bool) -> ('a -> unit) -> Result<'a, 'b> -> Result<'a, 'b>
```

### teeErrorIf

If the result is Error and the predicate returns true for the wrapped value, executes the function on the Error value. Passes through the input value unchanged.

```F#
('a -> bool) -> ('a -> unit) -> Result<'b, 'a> -> Result<'b, 'a>
```