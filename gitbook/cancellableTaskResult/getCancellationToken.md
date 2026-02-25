## CancellableTaskResult.getCancellationToken

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
unit -> CancellableTaskResult<CancellationToken, 'Error>
```

Gets the current `CancellationToken` from the surrounding `cancellableTaskResult` computation expression.

## Examples

Note: Many use-cases requiring the cancellation token can also be solved using [the `cancellableTaskResult` computation expression](../cancellableTaskResult/ce.md).

### Example 1

Passing the cancellation token to a cancellable database operation:

```fsharp
let fetchUser (userId: UserId) : CancellableTaskResult<User, string> =
    cancellableTaskResult {
        let! ct = CancellableTaskResult.getCancellationToken()
        let! user = db.Users.FindAsync(userId, ct) |> Task.map (Result.requireSome "User not found")
        return user
    }
```

### Example 2

Using the token to cancel an HTTP request:

```fsharp
let downloadFile (url: string) : CancellableTaskResult<byte[], exn> =
    cancellableTaskResult {
        let! ct = CancellableTaskResult.getCancellationToken()
        let! response = httpClient.GetAsync(url, ct) |> Task.mapResult id
        let! content = response.Content.ReadAsByteArrayAsync(ct) |> Task.mapResult id
        return content
    }
```

### Example 3

Passing the token to multiple operations within the same computation:

```fsharp
let processOrder (orderId: OrderId) : CancellableTaskResult<Receipt, string> =
    cancellableTaskResult {
        let! ct = CancellableTaskResult.getCancellationToken()
        let! order = fetchOrder orderId ct
        let! inventory = checkInventory order.Items ct
        let! receipt = placeOrder order inventory ct
        return receipt
    }
```
