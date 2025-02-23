## Task.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c) -> Task<'a> -> Task<'b> -> Task<'c>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `task` computation expression](../task/ce.md).

### Example 1

Given the functions

```fsharp
getFollowerIds : UserId -> Task<UserId list>
createPost : CreatePostRequest -> Task<PostId>
```

And the type

```fsharp
type NotifyNewPostRequest = 
  { UserIds : UserId list
    NewPostId : PostId }
  static member Create userIds newPostsId =
    {UserIds = userIds; NewPostId = newPostsId}
```

We can create a `NotifyNewPostRequest` using `Task.map2` as below:

```fsharp
let createPostAndGetNotifyRequest (req : CreatePostRequest) = 
  //  Task<UserId list>
  let getFollowersResult = getFollowerIds req.UserId

  // Task<PostId>
  let createPostResult = createPost req

  // Task<NotifyNewPostRequest>
  let newPostRequestResult =
    Task.map2 
      NotifyNewPostRequest.Create getFollowersResult createPostResult

  // ...
```

