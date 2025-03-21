## ParallelAsyncResult.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c) -> Async<Result<'a, 'd>> -> Async<Result<'b, 'd>>
  -> Async<Result<'c, 'd>>
```

## Examples

Note: Many use-cases requiring `map2` operations can also be solved using [the `parallelAsyncResult` computation expression](../parallelAsyncResult/ce.md).

### Example 1

Given the functions

```fsharp
getFollowerIds : UserId -> Async<Result<UserId list, exn>>
createPost : CreatePostRequest -> Async<Result<PostId, exn>>
```

And the type

```fsharp
type NotifyNewPostRequest =
  { UserIds : UserId list
    NewPostId : PostId }
  static member Create userIds newPostsId =
    {UserIds = userIds; NewPostId = newPostsId}
```

We can create a `NotifyNewPostRequest` using `ParallelAsyncResult.map2` as below:

```fsharp
let createPostAndGetNotifyRequest (req : CreatePostRequest) =
  //  Async<Result<UserId list, exn>>
  let getFollowersResult = getFollowerIds req.UserId

  // Async<Result<PostId, exn>>
  let createPostResult = createPost req

  // Async<Result<NotifyNewPostRequest, exn>>
  let newPostRequestResult =
    ParallelAsyncResult.map2
      NotifyNewPostRequest.Create getFollowersResult createPostResult

  // ...
```

This workflow will run the sub-tasks `getFollowersResult` and `createPostResult` concurrently, which can increase throughput.