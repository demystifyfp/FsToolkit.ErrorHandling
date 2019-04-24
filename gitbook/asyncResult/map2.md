## AsyncResult.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c) -> Async<Result<'a, 'd>> -> Async<Result<'b, 'd>> 
  -> Async<Result<'c, 'd>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `asyncResult` computation expression](../asyncResult/ce.md).

### Example 1

Given the functions

```fsharp
getFollowerIds : UserId -> Async<Result<UserId, exn>>
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

We can create a `NotifyNewPostRequest` using `AsyncResult.map2` as below:

```fsharp
let createPostAndGetNotifyRequest (req : CreatePostRequest) = 
  //  Async<Result<UserId, exn>>
  let getFollowersResult = getFollowerIds req.UserId

  // Async<Result<PostId, exn>>
  let createPostResult = createPost req

  // Async<Result<NotifyNewPostRequest, exn>>
  let newPostRequestResult =
    AsyncResult.map2 
      NotifyNewPostRequest.Create getFollowersResult createPostResult

  // ...
```

