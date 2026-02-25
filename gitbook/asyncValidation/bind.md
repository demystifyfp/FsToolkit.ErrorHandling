# AsyncValidation.bind

Namespace: `FsToolkit.ErrorHandling`

`bind` applies an async function to the value inside an `AsyncValidation` if it is `Ok`, returning the resulting `AsyncValidation`. Like `Validation.bind`, this uses **monadic (short-circuit) semantics** — if the input is an `Error`, the binder function is not called and the error is returned immediately.

## Function Signature

```fsharp
('okInput -> AsyncValidation<'okOutput, 'error>) -> AsyncValidation<'okInput, 'error> -> AsyncValidation<'okOutput, 'error>
```

## Examples

Take the following function for example:

```fsharp
// string -> AsyncValidation<int, string>
let tryParseIntAsync (s: string) =
    async {
        match System.Int32.TryParse(s) with
        | true, i -> return Ok i
        | false, _ -> return Error [ $"'%s{s}' is not a valid integer" ]
    }
```

### Example 1

```fsharp
let result =
    AsyncValidation.ok "42"
    |> AsyncValidation.bind tryParseIntAsync

// async { Ok 42 }
```

### Example 2

```fsharp
let result =
    AsyncValidation.ok "not a number"
    |> AsyncValidation.bind tryParseIntAsync

// async { Error ["'not a number' is not a valid integer"] }
```

### Example 3

```fsharp
let result =
    AsyncValidation.error "previous error"
    |> AsyncValidation.bind tryParseIntAsync

// async { Error ["previous error"] }
```
