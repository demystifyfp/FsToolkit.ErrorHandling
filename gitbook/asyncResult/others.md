## Other Useful Functions

Ported from [Cvdm.ErrorHandling](https://github.com/cmeeren/Cvdm.ErrorHandling). Credits [Christer van der Meeren](https://github.com/cmeeren)


### requireTrue

Returns the specified error if the async-wrapped value is false.
```
'a -> Async<bool> -> Async<Result<unit, 'a>>`
```
### requireFalse

Returns the specified error if the async-wrapped value is true.
```
'a -> Async<bool> -> Async<Result<unit, 'a>>`
```

### requireSome

Converts an async-wrapped Option to a Result, using the given error if None.
```
'a -> Async<'b option> -> Async<Result<'b, 'a>>`
```
### requireNone

Converts an async-wrapped Option to a Result, using the given error if Some.

```
'a -> Async<'b option> -> Async<Result<'b, 'a>>`
```


### requireEquals

Returns Ok if the async-wrapped value and the provided value are equal, or the specified error if not.
  
```
'a -> 'b -> Async<'a> -> Async<Result<unit, 'b>>
```

### requireEmpty

Returns Ok if the async-wrapped sequence is empty, or the specified error if not.

```
'a -> Async<'b> -> Async<Result<unit, 'a>>
```

### requireNotEmpty

Returns Ok if the async-wrapped sequence is not-empty, or the specified error if not.

```
'a -> Async<'b> -> Async<Result<unit, 'a>>
```


### requireHead

Returns the first item of the sequence if it exists, or the specified
error if the sequence is empty

```
'a -> Async<'b> -> Async<Result<'c, 'a>>
```


### setError

Replaces an error value of an async-wrapped result with a custom error value

```
'a -> Async<Result<'b, 'c>> -> Async<Result<'b, 'a>>
```

### withError

Replaces a unit error value of an async-wrapped result with a custom error value. Safer than setError since you're not losing any information.

```
'a -> Async<Result<'b, uni>t> -> Async<Result<'b, 'a>>
```

### defaultValue

Extracts the contained value of an async-wrapped result if Ok, otherwise uses ifError `'a`

```
'a -> Async<Result<'a, 'b>> -> Async<'a>
```

### defaultWith

Extracts the contained value of an async-wrapped result if Ok, otherwise evaluates ifErrorThunk and uses the result.

```
(unit -> 'a) -> Async<Result<'a, 'b>> -> Async<'a>
```


### ignoreError

Same as defaultValue for a result where the Ok value is unit. The name describes better what is actually happening in this case.

```
Async<Result<unit, 'a>> -> Async<unit>
```

### tee
If the async-wrapped result is Ok, executes the function on the Ok value. Passes through the input value.

```
('a -> unit) -> Async<Result<'a, 'b>> -> Async<Result<'a, 'b>>
```

### teeError

If the async-wrapped result is Error, executes the function on the Error value. Passes through the input value.

```
('a -> unit) -> Async<Result<'b, 'a>> -> Async<Result<'b, 'a>>
```

### teeIf

If the async-wrapped result is Ok and the predicate returns true, executes the function on the Ok value. Passes through the input value.

```
('a -> bool) -> ('a -> unit) -> Async<Result<'a, 'b>> -> Async<Result<'a, 'b>>
```

### teeErrorIf

If the async-wrapped result is Error and the predicate returns true, executes the function on the Error value. Passes through the input value.

```
('a -> bool) -> ('a -> unit) -> Async<Result<'b, 'a>> -> Async<Result<'b, 'a>>
```

