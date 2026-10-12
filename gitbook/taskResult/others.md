## Other Useful Functions

### req

Any `Req`.* function can be bound to a `Task`-wrapped `Result` via `TaskResult.req`
It feeds the inner `Ok` value through the checker, which can then accept and/or transform the value,
or return `Error` with the given error value if the condition is not met.

```fsharp
'checker -> 'errorOrErrorF -> Task<Result<'ok, 'error>> -> Task<Result<'output, 'error>>

TaskResult.req Req.notEmpty "TaskResult was not Ok <non-empty Seq>"

----

TaskResult.ok [| 1; 2; 3 |]
|> TaskResult.req Req.notEmpty "Result was not Ok <non-empty Seq>"
// => Ok ()

TaskResult.ok Seq.empty
|> TaskResult.req Req.notEmpty "Result was not Ok <non-empty Seq>"
// => Error "Result was not Ok <non-empty Seq>"
```

### reqFilter

Unpacks the `Task`'s `Result`, yielding the value (wrapped in `Ok`) if the `predicate` accepts it.
If the Result is Error or the predicate returns `false`, it yields an `Error` with the specified `error`.

```fsharp
('ok -> bool) -> 'error -> Task<Result<'ok,'error>> -> Task<Result<'ok,'error>>
```

### setError

Replaces an error value of an task-wrapped result with a custom error value

```fsharp
'a -> Task<Result<'b, 'c>> -> Task<Result<'b, 'a>>
```

### withError

Replaces a unit error value of an task-wrapped result with a custom error value. Safer than `setError` since you're not losing any information.

```fsharp
'a -> Task<Result<'b, unit>> -> Task<Result<'b, 'a>>
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

### defaultError

Extracts the contained error value of a task-wrapped result if `Error`, otherwise uses the provided value.

```fsharp
'error -> Task<Result<'ok, 'error>> -> Task<'error>
```
