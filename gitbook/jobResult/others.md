## Other Useful Functions

### req

Any `Req`.* function can be bound to a `Job`-wrapped `Result` via `JobResult.req`
It feeds the inner `Ok` value through the checker, which can then accept and/or transform the value,
or return `Error` with the given error value if the condition is not met.

```fsharp
'checker -> 'errorOrErrorF -> Job<Result<'ok, 'error>> -> Job<Result<'output, 'error>>

JobResult.req Req.notEmpty "JobResult was not Ok <non-empty Seq>"

----

JobResult.ok [| 1; 2; 3 |] |> JobResult.req Req.notEmpty "Result was not Ok <non-empty Seq>"
// => Ok ()
JobResult.ok Seq.empty |> JobResult.req Req.notEmpty "Result was not Ok <non-empty Seq>"
// => Error "Result was not Ok <non-empty Seq>"
```

### reqFilter

Unpacks the `Job`'s `Result`, yielding the value (wrapped in `Ok`) if the `predicate` accepts it.
If the Result is Error or the predicate returns `false`, it yields an `Error` with the specified `error`.

```fsharp
('ok -> bool) -> 'error -> Job<Result<'ok,'error>> -> Job<Result<'ok,'error>>
```

### setError

Replaces an error value of an job-wrapped result with a custom error value

```fsharp
'a -> Job<Result<'b, 'c>> -> Job<Result<'b, 'a>>
```

### withError

Replaces a unit error value of an job-wrapped result with a custom error value. Safer than `setError` since you're not losing any information.

```fsharp
'a -> Job<Result<'b, unit>> -> Job<Result<'b, 'a>>
```

### defaultValue

Extracts the contained value of an job-wrapped result if Ok, otherwise uses the provided value.

```fsharp
'a -> Job<Result<'a, 'b>> -> Job<'a>
```

### defaultWith

Extracts the contained value of an job-wrapped result if Ok, otherwise evaluates the given function and uses the result.

```fsharp
(unit -> 'a) -> Job<Result<'a, 'b>> -> Job<'a>
```

### ignoreError

Same as `defaultValue` for a result where the Ok value is unit. The name describes better what is actually happening in this case.

```fsharp
Job<Result<unit, 'a>> -> Job<unit>
```

### tee
If the job-wrapped result is Ok, executes the function on the Ok value. Passes through the input value unchanged.

```fsharp
('a -> unit) -> Job<Result<'a, 'b>> -> Job<Result<'a, 'b>>
```

### teeError

If the job-wrapped result is Error, executes the function on the Error value. Passes through the input value unchanged.

```fsharp
('a -> unit) -> Job<Result<'b, 'a>> -> Job<Result<'b, 'a>>
```

### teeIf

If the job-wrapped result is Ok and the predicate returns true for the wrapped value, executes the function on the Ok value. Passes through the input value unchanged.

```fsharp
('a -> bool) -> ('a -> unit) -> Job<Result<'a, 'b>> -> Job<Result<'a, 'b>>
```

### teeErrorIf

If the job-wrapped result is Error and the predicate returns true for the wrapped value, executes the function on the Error value. Passes through the input value unchanged.

```fsharp
('a -> bool) -> ('a -> unit) -> Job<Result<'a, 'b>> -> Job<Result<'a, 'b>>
```

### sequenceJob

Converts a `Result<Job<'a>, 'b>` to `Job<Result<'a, 'b>>`.

```fsharp
Result<Job<'a>, 'b> -> Job<Result<'a, 'b>>
```
