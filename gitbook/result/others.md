## Other Useful Functions

Ported from [Cvdm.ErrorHandling](https://github.com/cmeeren/Cvdm.ErrorHandling). Credits [Christer van der Meeren](https://github.com/cmeeren)

### requireTrue

Returns the specified error if the value is false.
```
'a -> bool -> Result<unit, 'a>`
```
### requireFalse

Returns the specified error if the value is true.
```
'a -> bool -> Result<unit, 'a>`
```


### requireSome

Converts an Option to a Result, using the given error if None
```
'a -> 'b option -> Result<'b, 'a>`
```
### requireNone

Converts an Option to a Result, using the given error if Some.
```
'a -> 'b option -> Result<'b, 'a>`
```

### requireEquals

Returns Ok if the two values are equal, or the specified error if not.
```
'a -> 'b -> 'a -> Result<unit, 'b>
```

### requireEmpty

Returns Ok if the sequence is empty, or the specified error if not.

```
'a -> seq<'b> -> Result<unit, 'a>
```

### requireNotEmpty

Returns the specified error if the sequence is empty, or Ok if not.

```
'a -> seq<'b> -> Result<unit, 'a>
```

### requireHead

Returns the first item of the sequence if it exists, or the specified
error if the sequence is empty

```
'a -> seq<'b> -> Result<'b, 'a>
```


### withError

Replaces an error value with a custom error value

```
'a -> Result<'b, 'c> -> Result<'b, 'a>
```

### defaultValue

Returns the contained value if Ok, otherwise returns the provided value

```
'a -> Result<'a, 'b> -> 'a
```

### defaultWith

Returns the contained value if Ok, otherwise evaluates the given function and returns the result.

```
(unit -> 'a) -> Result<'a, 'b> -> 'a
```


### ignoreError

Same as defaultValue for a result where the Ok value is unit. 

```
Result<unit, 'a> -> unit
```

### tee

If the result is Ok, executes the function on the Ok value. Passes through the input value unchanged.

```
('a -> unit) -> Result<'a, 'b> -> Result<'a, 'b>
```

### teeError

If the result is Error, executes the function on the Error value. Passes through the input value unchanged.

```
('a -> unit) -> Result<'b, 'a> -> Result<'b, 'a>
```

### teeIf

If the result is Ok and the predicate returns true for the Ok's value, executes the function on the Ok value. Passes through the input value unchanged.

```
('a -> bool) -> ('a -> unit) -> Result<'a, 'b> -> Result<'a, 'b>
```

### teeErrorIf

If the result is Error and the predicate returns true for the Error's value, executes the function on the Error value. Passes through the input value unchanged.

```
('a -> bool) -> ('a -> unit) -> Result<'b, 'a> -> Result<'b, 'a>
```