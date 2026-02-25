## CancellableTaskValidation.mapErrors

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('errorInput list -> 'errorOutput list)
  -> CancellableTaskValidation<'ok, 'errorInput>
  -> CancellableTaskValidation<'ok, 'errorOutput>
```

Applies a mapping function to the **entire error list** inside a `CancellableTaskValidation`. If the computation is `Ok`, the mapping function is not called and the value is propagated unchanged.

Note: `mapErrors` operates on the whole list at once. To map over individual errors, use [`mapError`](../cancellableTaskValidation/mapError.md).

## Examples

### Example 1

Deduplicating errors before returning them:

```fsharp
let validateInput (input: FormInput) : CancellableTaskValidation<ValidatedForm, string> =
    runValidation input
    |> CancellableTaskValidation.mapErrors List.distinct
```

### Example 2

Sorting and limiting the number of errors returned to a caller:

```fsharp
let validateOrder (order: Order) : CancellableTaskValidation<ValidatedOrder, string> =
    runOrderValidation order
    |> CancellableTaskValidation.mapErrors (fun errs -> errs |> List.sort |> List.truncate 5)
```

### Example 3

Converting a list of typed errors to a list of user-facing messages:

```fsharp
let processRequest (req: Request) : CancellableTaskValidation<Response, string> =
    validateRequest req
    |> CancellableTaskValidation.mapErrors (List.map (fun (e: DomainError) -> e.UserMessage))
```
