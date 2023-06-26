## CancellableTaskResult.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> CancellableTask<Result<'a, 'c>> -> CancellableTask<Result<'b, 'c>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `cancellableTaskResult` computation expression](../cancellableTaskResult/ce.md).

### Example 1

As a continuation of [Result.map3 Example 2](../result/map3.md#example-2), let's assume that we want to store the created post in the database using the function

```fsharp
savePost : CreatePostRequest -> CancellableTask<Result<PostId, exn>>
```

We can save the post and return its inner using `CancellableTaskResult.map`:

```fsharp
let rawPostId : CancellableTask<Result<Guid, exn>> =
  savePost createPostRequest
  |> CancellableTaskResult.map (fun (PostId postId) -> postId)
```

