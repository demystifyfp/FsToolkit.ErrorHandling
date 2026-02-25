## CancellableTaskValidation.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('input -> CancellableTaskValidation<'output, 'Error>)
  -> CancellableTaskValidation<'input, 'Error>
  -> CancellableTaskValidation<'output, 'Error>
```

Chains two `CancellableTaskValidation` computations using **monadic** (short-circuit) semantics. If the first computation returns an `Error`, the binder function is not called and the error is propagated immediately.

Note: Unlike [`apply`](../cancellableTaskValidation/apply.md) or [`map2`](../cancellableTaskValidation/map2.md), `bind` does **not** accumulate multiple errors — it short-circuits on the first failure. For error accumulation, use the applicative operators or the `cancellableTaskValidation` CE with `and!`.

## Examples

Note: Many use-cases requiring `bind` operations can also be solved using [the `cancellableTaskValidation` computation expression](../cancellableTaskValidation/ce.md).

### Example 1

Sequentially validating a user exists before validating their permissions:

```fsharp
let validateUserWithPermissions (userId: UserId) : CancellableTaskValidation<UserWithRole, string> =
    fetchUser userId
    |> CancellableTaskValidation.bind (fun user -> fetchAndValidateRole user)
```

### Example 2

Chaining validation steps in a pipeline:

```fsharp
let processPayment (request: PaymentRequest) : CancellableTaskValidation<Receipt, PaymentError> =
    validatePaymentRequest request
    |> CancellableTaskValidation.bind authorizePayment
    |> CancellableTaskValidation.bind chargeCard
```

### Example 3

Using `bind` inside a `cancellableTaskValidation` CE with sequential steps:

```fsharp
let createUser (input: CreateUserInput) : CancellableTaskValidation<UserId, string> =
    cancellableTaskValidation {
        let! validatedEmail = validateEmail input.Email
        // bind is used implicitly here — short-circuits if validateEmail fails
        let! userId = createUserRecord validatedEmail input.Name
        return userId
    }
```
