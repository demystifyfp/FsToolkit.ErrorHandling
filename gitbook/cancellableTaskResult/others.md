## Other Useful Functions

### req

Any `Req`.* function can be bound to a `CancellableTask`-wrapped `Result` via `CancellableTaskResult.req`
It feeds the inner `Ok` value through the checker, which can then accept and/or transform the value,
or return `Error` with the given error value if the condition is not met.

```fsharp
'checker -> 'errorOrErrorF -> CancellableTask<Result<'ok, 'error>> -> CancellableTask<Result<'output, 'error>>

CancellableTaskResult.req Req.notEmpty "CancellableTaskResult was not Ok <non-empty Seq>"

----

CancellableTaskResult.ok [| 1; 2; 3 |]
|> CancellableTaskResult.req Req.notEmpty "Result was not Ok <non-empty Seq>"
// => Ok ()

CancellableTaskResult.ok Seq.empty
|> CancellableTaskResult.req Req.notEmpty "Result was not Ok <non-empty Seq>"
// => Error "Result was not Ok <non-empty Seq>"
```

### reqFilter

Unpacks the `CancellableTask`'s `Result`, yielding the value (wrapped in `Ok`) if the `predicate` accepts it.
If the Result is Error or the predicate returns `false`, it yields an `Error` with the specified `error`.

```fsharp
('ok -> bool) -> 'error -> CancellableTask<Result<'ok,'error>> -> CancellableTask<Result<'ok,'error>>
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
