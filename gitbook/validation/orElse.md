# Validation.orElse / Validation.orElseWith

Namespace: `FsToolkit.ErrorHandling`

## orElse

`orElse` returns the original `Validation` if it is `Ok`, otherwise returns the provided alternative `Validation`.

### Function Signature

```fsharp
Validation<'ok, 'errorOutput> -> Validation<'ok, 'errorInput> -> Validation<'ok, 'errorOutput>
```

### Examples

#### Example 1

```fsharp
let result =
    Validation.ok "First"
    |> Validation.orElse (Validation.ok "Second")

// Ok "First"
```

#### Example 2

```fsharp
let result =
    Validation.error "First"
    |> Validation.orElse (Validation.ok "Second")

// Ok "Second"
```

#### Example 3

```fsharp
let result =
    Validation.error "First"
    |> Validation.orElse (Validation.error "Second")

// Error ["Second"]
```

---

## orElseWith

`orElseWith` returns the original `Validation` if it is `Ok`, otherwise calls the given function with the error list and returns its result.

### Function Signature

```fsharp
('errorInput list -> Validation<'ok, 'errorOutput>) -> Validation<'ok, 'errorInput> -> Validation<'ok, 'errorOutput>
```

### Examples

#### Example 1

```fsharp
let result =
    Validation.ok "First"
    |> Validation.orElseWith (fun _ -> Validation.ok "Second")

// Ok "First"
```

#### Example 2

```fsharp
let result =
    Validation.error "First"
    |> Validation.orElseWith (fun _ -> Validation.ok "Second")

// Ok "Second"
```

#### Example 3

```fsharp
let result =
    Validation.error "First"
    |> Validation.orElseWith (fun errors ->
        Validation.error $"Recovered from: %A{errors}")

// Error ["Recovered from: [\"First\"]"]
```
