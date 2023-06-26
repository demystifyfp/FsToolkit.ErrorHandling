## Other Useful Functions


### requireTrue

Returns the specified error if the task-wrapped value is `false`.
```fsharp
'a -> CancellableTask<bool> -> CancellableTask<Result<unit, 'a>>`
```
### requireFalse

Returns the specified error if the task-wrapped value is `true`.
```fsharp
'a -> CancellableTask<bool> -> CancellableTask<Result<unit, 'a>>`
```

### requireSome

Converts an task-wrapped Option to a Result, using the given error if None.
```fsharp
'a -> CancellableTask<'b option> -> CancellableTask<Result<'b, 'a>>`
```
### requireNone

Converts an task-wrapped Option to a Result, using the given error if Some.

```fsharp
'a -> CancellableTask<'b option> -> CancellableTask<Result<unit, 'a>>`
```


### requireEqual

Returns Ok if the task-wrapped value and the provided value are equal, or the specified error if not. Same as `requireEqualTo`, but with a parameter order that fits normal function application better than piping.

```fsharp
'a -> CancellableTask<'a> -> 'b -> CancellableTask<Result<unit, 'b>>
```

### requireEqualTo

Returns Ok if the task-wrapped value and the provided value are equal, or the specified error if not. Same as `requireEqual`, but with a parameter order that fits piping better than normal function application.

```fsharp
'a -> 'b -> CancellableTask<'a> -> CancellableTask<Result<unit, 'b>>
```

### requireEmpty

Returns Ok if the task-wrapped sequence is empty, or the specified error if not.

```fsharp
'a -> CancellableTask<'b> -> CancellableTask<Result<unit, 'a>>
```

### requireNotEmpty

Returns Ok if the task-wrapped sequence is non-empty, or the specified error if not.

```fsharp
'a -> CancellableTask<'b> -> CancellableTask<Result<unit, 'a>>
```


### requireHead

Returns the first item of the sequence if it exists, or the specified error if the sequence is empty

```fsharp
'a -> CancellableTask<'b> -> CancellableTask<Result<'c, 'a>>
```


### setError

Replaces an error value of an task-wrapped result with a custom error value

```fsharp
'a -> CancellableTask<Result<'b, 'c>> -> CancellableTask<Result<'b, 'a>>
```

### withError

Replaces a unit error value of an task-wrapped result with a custom error value. Safer than `setError` since you're not losing any information.

```fsharp
'a -> CancellableTask<Result<'b, unit>> -> CancellableTask<Result<'b, 'a>>
```

### defaultValue

Extracts the contained value of an task-wrapped result if Ok, otherwise uses the provided value.

```fsharp
'a -> CancellableTask<Result<'a, 'b>> -> CancellableTask<'a>
```

### defaultWith

Extracts the contained value of an task-wrapped result if Ok, otherwise evaluates the given function and uses the result.

```fsharp
(unit -> 'a) -> CancellableTask<Result<'a, 'b>> -> CancellableTask<'a>
```

### ignoreError

Same as `defaultValue` for a result where the Ok value is unit. The name describes better what is actually happening in this case.

```fsharp
CancellableTask<Result<unit, 'a>> -> CancellableTask<unit>
```

### tee
If the task-wrapped result is Ok, executes the function on the Ok value. Passes through the input value unchanged.

```fsharp
('a -> unit) -> CancellableTask<Result<'a, 'b>> -> CancellableTask<Result<'a, 'b>>
```

### teeError

If the task-wrapped result is Error, executes the function on the Error value. Passes through the input value unchanged.

```fsharp
('a -> unit) -> CancellableTask<Result<'b, 'a>> -> CancellableTask<Result<'b, 'a>>
```

### teeIf

If the task-wrapped result is Ok and the predicate returns true for the wrapped value, executes the function on the Ok value. Passes through the input value unchanged.

```fsharp
('a -> bool) -> ('a -> unit) -> CancellableTask<Result<'a, 'b>> -> CancellableTask<Result<'a, 'b>>
```

### teeErrorIf

If the task-wrapped result is Error and the predicate returns true for the wrapped value, executes the function on the Error value. Passes through the input value unchanged.

```fsharp
('a -> bool) -> ('a -> unit) -> CancellableTask<Result<'b, 'a>> -> CancellableTask<Result<'b, 'a>>
```

### sequenceTask

Converts a `Result<CancellableTask<'a>, 'b>` to `CancellableTask<Result<'a, 'b>>`.

```fsharp
Result<CancellableTask<'a>, 'b> -> CancellableTask<Result<'a, 'b>>
```
