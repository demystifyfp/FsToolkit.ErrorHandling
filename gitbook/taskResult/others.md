## Other Useful Functions


### requireTrue

Returns the specified error if the task-wrapped value is `false`.
```fsharp
'a -> Task<bool> -> Task<Result<unit, 'a>>`
```
### requireFalse

Returns the specified error if the task-wrapped value is `true`.
```fsharp
'a -> Task<bool> -> Task<Result<unit, 'a>>`
```

### requireSome

Converts an task-wrapped Option to a Result, using the given error if None.
```fsharp
'a -> Task<'b option> -> Task<Result<'b, 'a>>`
```
### requireNone

Converts an task-wrapped Option to a Result, using the given error if Some.

```fsharp
'a -> Task<'b option> -> Task<Result<unit, 'a>>`
```


### requireEqual

Returns Ok if the task-wrapped value and the provided value are equal, or the specified error if not. Same as `requireEqualTo`, but with a parameter order that fits normal function application better than piping.

```fsharp
'a -> Task<'a> -> 'b -> Task<Result<unit, 'b>>
```

### requireEqualTo

Returns Ok if the task-wrapped value and the provided value are equal, or the specified error if not. Same as `requireEqual`, but with a parameter order that fits piping better than normal function application.

```fsharp
'a -> 'b -> Task<'a> -> Task<Result<unit, 'b>>
```

### requireEmpty

Returns Ok if the task-wrapped sequence is empty, or the specified error if not.

```fsharp
'a -> Task<'b> -> Task<Result<unit, 'a>>
```

### requireNotEmpty

Returns Ok if the task-wrapped sequence is non-empty, or the specified error if not.

```fsharp
'a -> Task<'b> -> Task<Result<unit, 'a>>
```


### requireHead

Returns the first item of the sequence if it exists, or the specified error if the sequence is empty

```fsharp
'a -> Task<'b> -> Task<Result<'c, 'a>>
```


### setError

Replaces an error value of an task-wrapped result with a custom error value

```fsharp
'a -> Task<Result<'b, 'c>> -> Task<Result<'b, 'a>>
```

### withError

Replaces a unit error value of an task-wrapped result with a custom error value. Safer than `setError` since you're not losing any information.

```fsharp
'a -> Task<Result<'b, uni>t> -> Task<Result<'b, 'a>>
```

### defaultValue

Extracts the contained value of an task-wrapped result if Ok, otherwise uses the provided value.

```fsharp
'a -> Task<Result<'a, 'b>> -> Task<'a>
```

### defaultWith

Extracts the contained value of an task-wrapped result if Ok, otherwise evaluates the given function and uses the result.

```fsharp
(unit -> 'a) -> Task<Result<'a, 'b>> -> Task<'a>
```

### ignoreError

Same as `defaultValue` for a result where the Ok value is unit. The name describes better what is actually happening in this case.

```fsharp
Task<Result<unit, 'a>> -> Task<unit>
```

### tee
If the task-wrapped result is Ok, executes the function on the Ok value. Passes through the input value unchanged.

```fsharp
('a -> unit) -> Task<Result<'a, 'b>> -> Task<Result<'a, 'b>>
```

### teeError

If the task-wrapped result is Error, executes the function on the Error value. Passes through the input value unchanged.

```fsharp
('a -> unit) -> Task<Result<'b, 'a>> -> Task<Result<'b, 'a>>
```

### teeIf

If the task-wrapped result is Ok and the predicate returns true for the wrapped value, executes the function on the Ok value. Passes through the input value unchanged.

```fsharp
('a -> bool) -> ('a -> unit) -> Task<Result<'a, 'b>> -> Task<Result<'a, 'b>>
```

### teeErrorIf

If the task-wrapped result is Error and the predicate returns true for the wrapped value, executes the function on the Error value. Passes through the input value unchanged.

```fsharp
('a -> bool) -> ('a -> unit) -> Task<Result<'b, 'a>> -> Task<Result<'b, 'a>>
```

### sequenceTask

Converts a `Result<Task<'a>, 'b>` to `Task<Result<'a, 'b>>`.

```fsharp
Result<Task<'a>, 'b> -> Task<Result<'a, 'b>>
```
