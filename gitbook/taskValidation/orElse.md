# TaskValidation.orElse / TaskValidation.orElseWith

Namespace: `FsToolkit.ErrorHandling`

## orElse

`orElse` returns the original `TaskValidation` if it is `Ok`, otherwise returns the provided alternative `TaskValidation`.

### Function Signature

```fsharp
TaskValidation<'ok, 'errorOutput> -> TaskValidation<'ok, 'errorInput> -> TaskValidation<'ok, 'errorOutput>
```

### Examples

#### Example 1

```fsharp
let result =
    TaskValidation.ok "First"
    |> TaskValidation.orElse (TaskValidation.ok "Second")

// task { Ok "First" }
```

#### Example 2

```fsharp
let result =
    TaskValidation.error "First"
    |> TaskValidation.orElse (TaskValidation.ok "Second")

// task { Ok "Second" }
```

#### Example 3

```fsharp
let result =
    TaskValidation.error "First"
    |> TaskValidation.orElse (TaskValidation.error "Second")

// task { Error ["Second"] }
```

---

## orElseWith

`orElseWith` returns the original `TaskValidation` if it is `Ok`, otherwise calls the given function with the error list and returns its result.

### Function Signature

```fsharp
('errorInput list -> TaskValidation<'ok, 'errorOutput>) -> TaskValidation<'ok, 'errorInput> -> TaskValidation<'ok, 'errorOutput>
```

### Examples

#### Example 1

```fsharp
let result =
    TaskValidation.ok "First"
    |> TaskValidation.orElseWith (fun _ -> TaskValidation.ok "Second")

// task { Ok "First" }
```

#### Example 2

```fsharp
let result =
    TaskValidation.error "First"
    |> TaskValidation.orElseWith (fun _ -> TaskValidation.ok "Second")

// task { Ok "Second" }
```

#### Example 3

```fsharp
let result =
    TaskValidation.error "First"
    |> TaskValidation.orElseWith (fun errors ->
        TaskValidation.error $"Recovered from: %A{errors}")

// task { Error ["Recovered from: [\"First\"]"] }
```
