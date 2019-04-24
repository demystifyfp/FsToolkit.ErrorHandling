## Other Useful Functions


### requireTrue

Returns the specified error if the async-wrapped value is `false`.
```fsharp
'a -> Async<bool> -> Async<Result<unit, 'a>>`
```
### requireFalse

Returns the specified error if the async-wrapped value is `true`.
```fsharp
'a -> Async<bool> -> Async<Result<unit, 'a>>`
```

### requireSome

Converts an async-wrapped Option to a Result, using the given error if None.
```fsharp
'a -> Async<'b option> -> Async<Result<'b, 'a>>`
```
### requireNone

Converts an async-wrapped Option to a Result, using the given error if Some.

```fsharp
'a -> Async<'b option> -> Async<Result<unit, 'a>>`
```


### requireEqual

Returns Ok if the async-wrapped value and the provided value are equal, or the specified error if not. Same as `requireEqualTo`, but with a parameter order that fits normal function application better than piping.

```fsharp
'a -> Async<'a> -> 'b -> Async<Result<unit, 'b>>
```

### requireEqualTo

Returns Ok if the async-wrapped value and the provided value are equal, or the specified error if not. Same as `requireEqual`, but with a parameter order that fits piping better than normal function application.

```fsharp
'a -> 'b -> Async<'a> -> Async<Result<unit, 'b>>
```

### requireEmpty

Returns Ok if the async-wrapped sequence is empty, or the specified error if not.

```fsharp
'a -> Async<'b> -> Async<Result<unit, 'a>>
```

### requireNotEmpty

Returns Ok if the async-wrapped sequence is non-empty, or the specified error if not.

```fsharp
'a -> Async<'b> -> Async<Result<unit, 'a>>
```


### requireHead

Returns the first item of the sequence if it exists, or the specified error if the sequence is empty

```fsharp
'a -> Async<'b> -> Async<Result<'c, 'a>>
```


### setError

Replaces an error value of an async-wrapped result with a custom error value

```fsharp
'a -> Async<Result<'b, 'c>> -> Async<Result<'b, 'a>>
```

### withError

Replaces a unit error value of an async-wrapped result with a custom error value. Safer than `setError` since you're not losing any information.

```fsharp
'a -> Async<Result<'b, uni>t> -> Async<Result<'b, 'a>>
```

### defaultValue

Extracts the contained value of an async-wrapped result if Ok, otherwise uses the provided value.

```fsharp
'a -> Async<Result<'a, 'b>> -> Async<'a>
```

### defaultWith

Extracts the contained value of an async-wrapped result if Ok, otherwise evaluates the given function and uses the result.

```fsharp
(unit -> 'a) -> Async<Result<'a, 'b>> -> Async<'a>
```

### ignoreError

Same as `defaultValue` for a result where the Ok value is unit. The name describes better what is actually happening in this case.

```fsharp
Async<Result<unit, 'a>> -> Async<unit>
```

### tee
If the async-wrapped result is Ok, executes the function on the Ok value. Passes through the input value unchanged.

```fsharp
('a -> unit) -> Async<Result<'a, 'b>> -> Async<Result<'a, 'b>>
```

### teeError

If the async-wrapped result is Error, executes the function on the Error value. Passes through the input value unchanged.

```fsharp
('a -> unit) -> Async<Result<'b, 'a>> -> Async<Result<'b, 'a>>
```

### teeIf

If the async-wrapped result is Ok and the predicate returns true for the wrapped value, executes the function on the Ok value. Passes through the input value unchanged.

```fsharp
('a -> bool) -> ('a -> unit) -> Async<Result<'a, 'b>> -> Async<Result<'a, 'b>>
```

### teeErrorIf

If the async-wrapped result is Error and the predicate returns true for the wrapped value, executes the function on the Error value. Passes through the input value unchanged.

```fsharp
('a -> bool) -> ('a -> unit) -> Async<Result<'b, 'a>> -> Async<Result<'b, 'a>>
```

