# AsyncValidation.orElse / AsyncValidation.orElseWith

Namespace: `FsToolkit.ErrorHandling`

## orElse

`orElse` returns the original `AsyncValidation` if it is `Ok`, otherwise returns the provided alternative `AsyncValidation`.

### Function Signature

```fsharp
AsyncValidation<'ok, 'errorOutput> -> AsyncValidation<'ok, 'errorInput> -> AsyncValidation<'ok, 'errorOutput>
```

### Examples

#### Example 1

```fsharp
let result =
    AsyncValidation.ok "First"
    |> AsyncValidation.orElse (AsyncValidation.ok "Second")

// async { Ok "First" }
```

#### Example 2

```fsharp
let result =
    AsyncValidation.error "First"
    |> AsyncValidation.orElse (AsyncValidation.ok "Second")

// async { Ok "Second" }
```

#### Example 3

```fsharp
let result =
    AsyncValidation.error "First"
    |> AsyncValidation.orElse (AsyncValidation.error "Second")

// async { Error ["Second"] }
```

---

## orElseWith

`orElseWith` returns the original `AsyncValidation` if it is `Ok`, otherwise calls the given function with the error list and returns its result.

### Function Signature

```fsharp
('errorInput list -> AsyncValidation<'ok, 'errorOutput>) -> AsyncValidation<'ok, 'errorInput> -> AsyncValidation<'ok, 'errorOutput>
```

### Examples

#### Example 1

```fsharp
let result =
    AsyncValidation.ok "First"
    |> AsyncValidation.orElseWith (fun _ -> AsyncValidation.ok "Second")

// async { Ok "First" }
```

#### Example 2

```fsharp
let result =
    AsyncValidation.error "First"
    |> AsyncValidation.orElseWith (fun _ -> AsyncValidation.ok "Second")

// async { Ok "Second" }
```

#### Example 3

```fsharp
let result =
    AsyncValidation.error "First"
    |> AsyncValidation.orElseWith (fun errors ->
        AsyncValidation.error $"Recovered from: %A{errors}")

// async { Error ["Recovered from: [\"First\"]"] }
```
