## Other Useful Functions


### requireTrue

Returns the specified error if the job-wrapped value is `false`.
```fsharp
'a -> job<bool> -> job<Result<unit, 'a>>`
```
### requireFalse

Returns the specified error if the job-wrapped value is `true`.
```fsharp
'a -> job<bool> -> job<Result<unit, 'a>>`
```

### requireSome

Converts an job-wrapped Option to a Result, using the given error if None.
```fsharp
'a -> job<'b option> -> job<Result<'b, 'a>>`
```
### requireNone

Converts an job-wrapped Option to a Result, using the given error if Some.

```fsharp
'a -> job<'b option> -> job<Result<unit, 'a>>`
```


### requireEqual

Returns Ok if the job-wrapped value and the provided value are equal, or the specified error if not. Same as `requireEqualTo`, but with a parameter order that fits normal function application better than piping.

```fsharp
'a -> job<'a> -> 'b -> job<Result<unit, 'b>>
```

### requireEqualTo

Returns Ok if the job-wrapped value and the provided value are equal, or the specified error if not. Same as `requireEqual`, but with a parameter order that fits piping better than normal function application.

```fsharp
'a -> 'b -> job<'a> -> job<Result<unit, 'b>>
```

### requireEmpty

Returns Ok if the job-wrapped sequence is empty, or the specified error if not.

```fsharp
'a -> job<'b> -> job<Result<unit, 'a>>
```

### requireNotEmpty

Returns Ok if the job-wrapped sequence is non-empty, or the specified error if not.

```fsharp
'a -> job<'b> -> job<Result<unit, 'a>>
```


### requireHead

Returns the first item of the sequence if it exists, or the specified error if the sequence is empty

```fsharp
'a -> job<'b> -> job<Result<'c, 'a>>
```


### setError

Replaces an error value of an job-wrapped result with a custom error value

```fsharp
'a -> job<Result<'b, 'c>> -> job<Result<'b, 'a>>
```

### withError

Replaces a unit error value of an job-wrapped result with a custom error value. Safer than `setError` since you're not losing any information.

```fsharp
'a -> job<Result<'b, uni>t> -> job<Result<'b, 'a>>
```

### defaultValue

Extracts the contained value of an job-wrapped result if Ok, otherwise uses the provided value.

```fsharp
'a -> job<Result<'a, 'b>> -> job<'a>
```

### defaultWith

Extracts the contained value of an job-wrapped result if Ok, otherwise evaluates the given function and uses the result.

```fsharp
(unit -> 'a) -> job<Result<'a, 'b>> -> job<'a>
```

### ignoreError

Same as `defaultValue` for a result where the Ok value is unit. The name describes better what is actually happening in this case.

```fsharp
job<Result<unit, 'a>> -> job<unit>
```

### tee
If the job-wrapped result is Ok, executes the function on the Ok value. Passes through the input value unchanged.

```fsharp
('a -> unit) -> job<Result<'a, 'b>> -> job<Result<'a, 'b>>
```

### teeError

If the job-wrapped result is Error, executes the function on the Error value. Passes through the input value unchanged.

```fsharp
('a -> unit) -> job<Result<'b, 'a>> -> job<Result<'b, 'a>>
```

### teeIf

If the job-wrapped result is Ok and the predicate returns true for the wrapped value, executes the function on the Ok value. Passes through the input value unchanged.

```fsharp
('a -> bool) -> ('a -> unit) -> job<Result<'a, 'b>> -> job<Result<'a, 'b>>
```

### teeErrorIf

If the job-wrapped result is Error and the predicate returns true for the wrapped value, executes the function on the Error value. Passes through the input value unchanged.

```fsharp
('a -> bool) -> ('a -> unit) -> job<Result<'b, 'a>> -> job<Result<'b, 'a>>
```

### sequenceJob

Converts a `Result<Job<'a>, 'b>` to `Job<Result<'a, 'b>>`.

```fsharp
Result<Job<'a>, 'b> -> Job<Result<'a, 'b>>
```

