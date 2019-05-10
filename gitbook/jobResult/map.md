## JobResult.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> Job<Result<'a, 'c>> -> Job<Result<'b, 'c>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `jobResult` computation expression](../jobResult/ce.md).

### Example 1

As a continuation of [Result.map3 Example 2](../result/map3.md#example-2), let's assume that we want to store the created post in the database using the function

```fsharp
savePost : CreatePostRequest -> Job<Result<PostId, exn>>
```

We can save the post and return its inner using `JobResult.map`:

```fsharp
let rawPostId : Job<Result<Guid, exn>> =
  savePost createostRequest
  |> JobResult.map (fun (PostId postId) -> postId)
```

