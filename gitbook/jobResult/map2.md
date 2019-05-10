## JobResult.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c) -> Job<Result<'a, 'd>> -> Job<Result<'b, 'd>> 
  -> Job<Result<'c, 'd>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `jobResult` computation expression](../jobResult/ce.md).

### Example 1

Given the functions

```fsharp
getFollowerIds : UserId -> Job<Result<UserId, exn>>
createPost : CreatePostRequest -> Job<Result<PostId, exn>>
```

And the type

```fsharp
type NotifyNewPostRequest = 
  { UserIds : UserId list
    NewPostId : PostId }
  static member Create userIds newPostsId =
    {UserIds = userIds; NewPostId = newPostsId}
```

We can create a `NotifyNewPostRequest` using `JobResult.map2` as below:

```fsharp
let createPostAndGetNotifyRequest (req : CreatePostRequest) = 
  //  Job<Result<UserId, exn>>
  let getFollowersResult = getFollowerIds req.UserId

  // Job<Result<PostId, exn>>
  let createPostResult = createPost req

  // Job<Result<NotifyNewPostRequest, exn>>
  let newPostRequestResult =
    JobResult.map2 
      NotifyNewPostRequest.Create getFollowersResult createPostResult

  // ...
```

