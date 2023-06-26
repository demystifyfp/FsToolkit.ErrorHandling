## CancellableTaskResult.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c) -> CancellableTask<Result<'a, 'd>> -> CancellableTask<Result<'b, 'd>> 
  -> CancellableTask<Result<'c, 'd>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `cancellableTaskResult` computation expression](../cancellableTaskResult/ce.md).

### Example 1

Given the functions

```fsharp
getFollowerIds : UserId -> CancellableTask<Result<UserId list, exn>>
createPost : CreatePostRequest -> CancellableTask<Result<PostId, exn>>
```

And the type

```fsharp
type NotifyNewPostRequest = 
  { UserIds : UserId list
    NewPostId : PostId }
  static member Create userIds newPostsId =
    {UserIds = userIds; NewPostId = newPostsId}
```

We can create a `NotifyNewPostRequest` using `CancellableTaskResult.map2` as below:

```fsharp
let createPostAndGetNotifyRequest (req : CreatePostRequest) = 
  //  CancellableTask<Result<UserId list, exn>>
  let getFollowersResult = getFollowerIds req.UserId

  // CancellableTask<Result<PostId, exn>>
  let createPostResult = createPost req

  // CancellableTask<Result<NotifyNewPostRequest, exn>>
  let newPostRequestResult =
    CancellableTaskResult.map2 
      NotifyNewPostRequest.Create getFollowersResult createPostResult

  // ...
```

