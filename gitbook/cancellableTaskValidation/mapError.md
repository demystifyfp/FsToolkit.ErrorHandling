## CancellableTaskValidation.mapError

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('errorInput -> 'errorOutput)
  -> CancellableTaskValidation<'ok, 'errorInput>
  -> CancellableTaskValidation<'ok, 'errorOutput>
```

Applies a mapping function to **each error** in the error list inside a `CancellableTaskValidation`. If the computation is `Ok`, the mapping function is not called and the value is propagated unchanged.

Note: `mapError` applies the function element-wise to each error in the list. To transform the entire error list at once, use [`mapErrors`](../cancellableTaskValidation/mapErrors.md).

## Examples

### Example 1

Wrapping low-level errors in a domain error type:

```fsharp
let validateUser (input: UserInput) : CancellableTaskValidation<User, AppError> =
    fetchAndValidateUser input
    |> CancellableTaskValidation.mapError AppError.ValidationError
```

### Example 2

Converting errors to strings for display:

```fsharp
let getValidationMessages (userId: UserId) : CancellableTaskValidation<User, string> =
    fetchUser userId
    |> CancellableTaskValidation.mapError (fun (err: ValidationError) -> err.Message)
```

### Example 3

Lifting errors from one domain to another at a boundary:

```fsharp
let createOrder (request: OrderRequest) : CancellableTaskValidation<Order, OrderServiceError> =
    validateOrderRequest request
    |> CancellableTaskValidation.mapError OrderServiceError.fromValidationError
```
