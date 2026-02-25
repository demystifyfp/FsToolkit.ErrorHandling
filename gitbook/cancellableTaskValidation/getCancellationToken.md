## CancellableTaskValidation.getCancellationToken

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
unit -> CancellableTaskValidation<CancellationToken, 'Error>
```

Gets the current `CancellationToken` from the surrounding `cancellableTaskValidation` computation expression.

## Examples

### Example 1

Passing the cancellation token to a cancellable database call:

```fsharp
let fetchUser (userId: UserId) : CancellableTaskValidation<User, string> =
    cancellableTaskValidation {
        let! ct = CancellableTaskValidation.getCancellationToken()
        let! user = db.Users.FindAsync(userId, ct) |> Task.map (Result.requireSome ["User not found"])
        return user
    }
```

### Example 2

Using the token for an HTTP request with cancellation support:

```fsharp
let downloadContent (url: string) : CancellableTaskValidation<string, string> =
    cancellableTaskValidation {
        let! ct = CancellableTaskValidation.getCancellationToken()
        let! response = httpClient.GetAsync(url, ct) |> Task.mapValidation id
        let! content = response.Content.ReadAsStringAsync(ct) |> Task.mapValidation id
        return content
    }
```

### Example 3

Sharing the token across multiple operations in a validation workflow:

```fsharp
let validateAndSaveOrder (order: Order) : CancellableTaskValidation<OrderId, string> =
    cancellableTaskValidation {
        let! ct = CancellableTaskValidation.getCancellationToken()
        let! validatedItems = checkInventory order.Items ct
        let! validatedAddress = verifyAddress order.Address ct
        let! orderId = saveOrder validatedItems validatedAddress ct
        return orderId
    }
```
