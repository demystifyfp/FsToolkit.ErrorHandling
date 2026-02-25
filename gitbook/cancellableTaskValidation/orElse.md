## CancellableTaskValidation.orElse

Namespace: `FsToolkit.ErrorHandling`

Returns the original result unchanged if it is `Ok`, otherwise returns the provided alternative `CancellableTaskValidation`.

Note: `CancellableTaskValidation` accumulates errors as `'error list`. The `orElseWith` overload receives the full error list.

## Function Signature:

```fsharp
CancellableTaskValidation<'ok, 'errorOutput>
  -> CancellableTaskValidation<'ok, 'errorInput>
  -> CancellableTaskValidation<'ok, 'errorOutput>
```

## Examples

### Example 1

```fsharp
CancellableTaskValidation.error "First"
|> CancellableTaskValidation.orElse (CancellableTaskValidation.singleton "Second")
// evaluates to Ok "Second"
```

### Example 2

When the input is already `Ok`, the alternative is ignored:

```fsharp
CancellableTaskValidation.singleton "First"
|> CancellableTaskValidation.orElse (CancellableTaskValidation.error "Second")
// evaluates to Ok "First"
```

---

## CancellableTaskValidation.orElseWith

Namespace: `FsToolkit.ErrorHandling`

Returns the original result unchanged if it is `Ok`, otherwise calls the given function with the accumulated error list and returns its result. The alternative function is **not** evaluated unless the input is `Error`.

## Function Signature:

```fsharp
('errorInput list -> CancellableTaskValidation<'ok, 'errorOutput>)
  -> CancellableTaskValidation<'ok, 'errorInput>
  -> CancellableTaskValidation<'ok, 'errorOutput>
```

## Examples

### Example 1

```fsharp
CancellableTaskValidation.error "First"
|> CancellableTaskValidation.orElseWith (fun _ -> CancellableTaskValidation.error "Second")
// evaluates to Error ["Second"]
```

### Example 2

Falling back to a secondary validation strategy when the primary fails:

```fsharp
let validateInput (input: string) : CancellableTaskValidation<ParsedInput, string> =
  strictValidation input
  |> CancellableTaskValidation.orElseWith (fun errs ->
      lenientValidation input)
```
