## CancellableTaskValidation.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('input -> 'output)
  -> CancellableTaskValidation<'input, 'error>
  -> CancellableTaskValidation<'output, 'error>
```

Applies a mapping function to the `Ok` value inside a `CancellableTaskValidation`. If the computation contains an `Error`, the mapping function is not called and the errors are propagated unchanged.

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `cancellableTaskValidation` computation expression](../cancellableTaskValidation/ce.md).

### Example 1

Transforming the inner value of a successful validation:

```fsharp
let postIdResult : CancellableTaskValidation<Guid, string> =
    savePost createPostRequest
    |> CancellableTaskValidation.map (fun (PostId postId) -> postId)
```

### Example 2

Mapping a domain type to a DTO for serialization:

```fsharp
let getUserDto (userId: UserId) : CancellableTaskValidation<UserDto, string> =
    fetchUser userId
    |> CancellableTaskValidation.map UserDto.ofDomain
```

### Example 3

Chaining multiple transformations:

```fsharp
let getDisplayName (userId: UserId) : CancellableTaskValidation<string, string> =
    fetchUser userId
    |> CancellableTaskValidation.map (fun user -> $"{user.FirstName} {user.LastName}")
```
