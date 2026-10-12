## Other Useful Functions

### req

Any `Req`.* function can be bound to an `Async`-wrapped `Result` via `AsyncResult.req`
It feeds the inner `Ok` value through the checker, which can then accept and/or transform the value,
or return `Error` with the given error value if the condition is not met.

```fsharp
'checker -> 'errorOrErrorF -> Async<Result<'ok, 'error>> -> Async<Result<'output, 'error>>

AsyncResult.req Req.notEmpty "AsyncResult was not Ok <non-empty Seq>"

----

AsyncResult.ok [| 1; 2; 3 |] |> AsyncResult.req Req.notEmpty "Result was not Ok <non-empty Seq>"
// => Ok ()
AsyncResult.ok Seq.empty |> AsyncResult.req Req.notEmpty "Result was not Ok <non-empty Seq>"
// => Error "Result was not Ok <non-empty Seq>"
```

### reqFilter

Unpacks the `Async`'s `Result`, yielding the value (wrapped in `Ok`) if the `predicate` accepts it.
If the Result is Error or the predicate returns `false`, it yields an `Error` with the specified `error`.

```fsharp
('ok -> bool) -> 'error -> Async<Result<'ok,'error>> -> Async<Result<'ok,'error>>
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
