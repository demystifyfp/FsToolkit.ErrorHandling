# Req Functions

### req

Any `Req`.* function can be bound to a `Result` via `Result.req`
It feeds the inner `Ok` value through the `checker`, which can then accept and/or transform the value,
or return `Error` with the given `error` value if the condition is not met.

```fsharp
'checker -> 'errorOrErrorF -> Result<'ok, 'error> -> Result<'output, 'error>

Result.req Req.notEmpty "Result was not Ok <non-empty Seq>"

----

Ok [| 1; 2; 3 |] |> Result.req Req.notEmpty "Result was not Ok <non-empty Seq>"
// => Ok ()
Ok Seq.empty |> Result.req Req.notEmpty "Result was not Ok <non-empty Seq>"
// => Error "Result was not Ok <non-empty Seq>"
```

## reqFilter

### Function Signature

If the input result is `Ok`, applies a predicate to the `Ok` value.
If the predicate returns true, then returns the original `Ok` Result.
Otherwise, returns a new `Error` result with the provided error.

NOTE: `Result.reqFilter predicate` is simply a shortcut for `Result.req (Req.filter predicate)`.
it's provided to make validation code easier to scan (especially if the predicate is a messy lambda).

```fsharp
('ok -> bool) -> 'error -> Result<'ok,'error> -> Result<'ok,'error>
```

Note: 
If you find that you need the Ok value to produce an appropriate error, use the `check` method instead.
Alternately, write your own `Req` function that follows the `input` -> `Result<'output, 'error>` signature.

#### Example 1

```fsharp
let result: Result<string, string> = 
    Ok "F#"
    |> Result.reqFilter _.Contains("#") "Provided input does not contain #"

// Ok "F#"
```

#### Example 2

```fsharp
let result: Result<string, string> = 
    Ok "Hello World!"
    |> Result.reqFilter _.Contains("#") "Provided input does not contain #"

// Error "Provided input does not contain #"
```
