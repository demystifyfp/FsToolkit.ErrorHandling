## CancellableTaskResult.parallelZip

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
CancellableTaskResult<'left, 'Error>
  -> CancellableTaskResult<'right, 'Error>
  -> CancellableTaskResult<'left * 'right, 'Error>
```

Takes two `CancellableTaskResult` values, starts them **concurrently**, and returns a tuple of the results once both complete. If either computation returns an `Error`, that error is returned.

Unlike [`zip`](../cancellableTaskResult/zip.md), both computations are started before waiting for either to complete, which can improve throughput when the two operations are independent.

## Examples

### Example 1

Fetching two independent resources in parallel:

```fsharp
let getUserAndPermissions (userId: UserId) : CancellableTaskResult<User * Permission list, string> =
    CancellableTaskResult.parallelZip
        (fetchUser userId)
        (fetchPermissions userId)
```

### Example 2

Combining two concurrent API calls in a larger computation:

```fsharp
let loadDashboard (userId: UserId) : CancellableTaskResult<Dashboard, string> =
    cancellableTaskResult {
        let! user, settings =
            CancellableTaskResult.parallelZip
                (fetchUser userId)
                (fetchUserSettings userId)
        return buildDashboard user settings
    }
```

### Example 3

Running two independent validations concurrently:

```fsharp
let validateOrder (order: Order) : CancellableTaskResult<ValidatedItems * ValidatedAddress, string> =
    CancellableTaskResult.parallelZip
        (validateItems order.Items)
        (validateShippingAddress order.Address)
```
