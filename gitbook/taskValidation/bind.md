# TaskValidation.bind

Namespace: `FsToolkit.ErrorHandling`

`bind` applies a function to the value inside a `TaskValidation` if it is `Ok`, returning the resulting `TaskValidation`. Like `Validation.bind`, this uses **monadic (short-circuit) semantics** — if the input is an `Error`, the binder function is not called and the error is returned immediately.

## Function Signature

```fsharp
('okInput -> TaskValidation<'okOutput, 'error>) -> TaskValidation<'okInput, 'error> -> TaskValidation<'okOutput, 'error>
```

## Examples

Take the following function for example:

```fsharp
// string -> TaskValidation<int, string>
let tryParseIntTask (s: string) =
    task {
        match System.Int32.TryParse(s) with
        | true, i -> return Ok i
        | false, _ -> return Error [ $"'%s{s}' is not a valid integer" ]
    }
```

### Example 1

```fsharp
let result =
    TaskValidation.ok "42"
    |> TaskValidation.bind tryParseIntTask

// task { Ok 42 }
```

### Example 2

```fsharp
let result =
    TaskValidation.ok "not a number"
    |> TaskValidation.bind tryParseIntTask

// task { Error ["'not a number' is not a valid integer"] }
```

### Example 3

```fsharp
let result =
    TaskValidation.error "previous error"
    |> TaskValidation.bind tryParseIntTask

// task { Error ["previous error"] }
```
