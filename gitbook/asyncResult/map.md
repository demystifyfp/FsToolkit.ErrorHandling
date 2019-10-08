## AsyncResult.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> Async<Result<'a, 'c>> -> Async<Result<'b, 'c>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `asyncResult` computation expression](../asyncResult/ce.md).

### Example 1

As a continuation of [Result.map3 Example 2](../result/map3.md#example-2), let's assume that we want to store the created post in the database using the function

```fsharp
savePost : CreatePostRequest -> Async<Result<PostId, exn>>
```

We can save the post and return its inner using `AsyncResult.map`:

```fsharp
let rawPostId : Async<Result<Guid, exn>> =
  savePost createPostRequest
  |> AsyncResult.map (fun (PostId postId) -> postId)
```

