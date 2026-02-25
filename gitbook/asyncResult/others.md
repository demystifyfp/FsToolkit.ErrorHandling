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
### requireSomeWith

Converts an async-wrapped Option to a Result, using the given error factory if None. The error factory is only called when the value is `None`.
```fsharp
(unit -> 'a) -> Async<'b option> -> Async<Result<'b, 'a>>
```
### requireNone

Converts an async-wrapped Option to a Result, using the given error if Some.

```fsharp
'a -> Async<'b option> -> Async<Result<unit, 'a>>`
```

### requireNoneWith

Converts an async-wrapped Option to a Result, using the given error factory if Some. The error factory is only called when the value is `Some`.

```fsharp
(unit -> 'a) -> Async<'b option> -> Async<Result<unit, 'a>>
```

### requireValueSome

Converts an async-wrapped ValueOption to a Result, using the given error if ValueNone.
```fsharp
'a -> Async<'b voption> -> Async<Result<'b, 'a>>
```

### requireValueNone

Converts an async-wrapped ValueOption to a Result, using the given error if ValueSome.

```fsharp
'a -> Async<'b voption> -> Async<Result<unit, 'a>>
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

### require

Returns the provided async-wrapped result if it is Ok and the predicate is true, or if the async-wrapped result is Error.
If the predicate is false, returns a new async-wrapped Error result with the error value.

```fsharp
('a -> bool) -> 'b -> Async<Result<'a,'b>> -> Async<Result<'a,'b>>
```

### setError

Replaces an error value of an async-wrapped result with a custom error value

```fsharp
'a -> Async<Result<'b, 'c>> -> Async<Result<'b, 'a>>
```

### withError

Replaces a unit error value of an async-wrapped result with a custom error value. Safer than `setError` since you're not losing any information.

```fsharp
'a -> Async<Result<'b, unit> -> Async<Result<'b, 'a>>
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

### defaultError

Extracts the contained error value of an async-wrapped result if `Error`, otherwise uses the provided value.

```fsharp
'error -> Async<Result<'ok, 'error>> -> Async<'error>
```

---

## bindRequire Functions

The `bindRequire*` functions combine a `bind` and a `require` check in one step. They bind the inner `Ok` value through a function, then assert a condition on the result, returning `Error` with the given error value if the condition is not met.

### bindRequireSome

Binds the async result and requires the inner `Ok` value to be `Some`, returning `Error` with the given error if it is `None`.

```fsharp
'error -> Async<Result<'ok option, 'error>> -> Async<Result<'ok, 'error>>
```

### bindRequireNone

Binds the async result and requires the inner `Ok` value to be `None`, returning `Error` with the given error if it is `Some`.

```fsharp
'error -> Async<Result<'ok option, 'error>> -> Async<Result<unit, 'error>>
```

### bindRequireValueSome

Binds the async result and requires the inner `Ok` value to be `ValueSome`, returning `Error` with the given error if it is `ValueNone`.

```fsharp
'error -> Async<Result<'ok voption, 'error>> -> Async<Result<'ok, 'error>>
```

### bindRequireValueNone

Binds the async result and requires the inner `Ok` value to be `ValueNone`, returning `Error` with the given error if it is `ValueSome`.

```fsharp
'error -> Async<Result<'ok voption, 'error>> -> Async<Result<unit, 'error>>
```

### bindRequireTrue

Binds the async result and requires the inner `Ok` value to be `true`, returning `Error` with the given error if it is `false`.

```fsharp
'error -> Async<Result<bool, 'error>> -> Async<Result<unit, 'error>>
```

### bindRequireFalse

Binds the async result and requires the inner `Ok` value to be `false`, returning `Error` with the given error if it is `true`.

```fsharp
'error -> Async<Result<bool, 'error>> -> Async<Result<unit, 'error>>
```

### bindRequireNotNull

Binds the async result and requires the inner `Ok` value to be non-null, returning `Error` with the given error if it is `null`.

```fsharp
'error -> Async<Result<'ok, 'error>> -> Async<Result<'ok, 'error>>
```

### bindRequireEqual

Binds the async result and requires the inner `Ok` value to equal the provided value, returning `Error` with the given error if they differ.

```fsharp
'ok -> 'error -> Async<Result<'ok, 'error>> -> Async<Result<unit, 'error>>
```

### bindRequireEmpty

Binds the async result and requires the inner `Ok` sequence to be empty, returning `Error` with the given error if it is not.

```fsharp
'error -> Async<Result<#seq<'ok>, 'error>> -> Async<Result<unit, 'error>>
```

### bindRequireNotEmpty

Binds the async result and requires the inner `Ok` sequence to be non-empty, returning `Error` with the given error if it is empty.

```fsharp
'error -> Async<Result<#seq<'ok>, 'error>> -> Async<Result<unit, 'error>>
```

### bindRequireHead

Binds the async result and returns the first element of the inner `Ok` sequence, returning `Error` with the given error if the sequence is empty.

```fsharp
'error -> Async<Result<#seq<'ok>, 'error>> -> Async<Result<'ok, 'error>>
```

