## TaskResult.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c) -> Task<Result<'a, 'd>> -> Task<Result<'b, 'd>> 
  -> Task<Result<'c, 'd>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `taskResult` computation expression](../taskResult/ce.md).

### Example 1

Given the functions

```fsharp
getFollowerIds : UserId -> Task<Result<UserId, exn>>
createPost : CreatePostRequest -> Task<Result<PostId, exn>>
```

And the type

```fsharp
type NotifyNewPostRequest = 
  { UserIds : UserId list
    NewPostId : PostId }
  static member Create userIds newPostsId =
    {UserIds = userIds; NewPostId = newPostsId}
```

We can create a `NotifyNewPostRequest` using `TaskResult.map2` as below:

```fsharp
let createPostAndGetNotifyRequest (req : CreatePostRequest) = 
  //  Task<Result<UserId, exn>>
  let getFollowersResult = getFollowerIds req.UserId

  // Task<Result<PostId, exn>>
  let createPostResult = createPost req

  // Task<Result<NotifyNewPostRequest, exn>>
  let newPostRequestResult =
    TaskResult.map2 
      NotifyNewPostRequest.Create getFollowersResult createPostResult

  // ...
```

