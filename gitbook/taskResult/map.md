## TaskResult.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> Task<Result<'a, 'c>> -> Task<Result<'b, 'c>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `taskResult` computation expression](../taskResult/ce.md).

### Example 1

As a continuation of [Result.map3 Example 2](../result/map3.md#example-2), let's assume that we want to store the created post in the database using the function

```fsharp
savePost : CreatePostRequest -> Task<Result<PostId, exn>>
```

We can save the post and return its inner using `TaskResult.map`:

```fsharp
let rawPostId : Task<Result<Guid, exn>> =
  savePost createostRequest
  |> TaskResult.map (fun (PostId postId) -> postId)
```

