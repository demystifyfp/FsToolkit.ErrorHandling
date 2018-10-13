## AsyncResult.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b -> 'c) -> Async<Result<'a, 'd>> -> Async<Result<'b, 'd>> 
  -> Async<Result<'c, 'd>>
```

## Examples

### Example 1

```fsharp
// UserId -> Async<Result<UserId, Exception>>
let getFollowersIds userId = async {
  // ...
}

// CreatePostRequest -> Async<Result<PostId, Exception>>
let createPost (req : CreatePostRequest) = async {
  // ...
}

type NotifyNewPostRequest = {
  UserIds : UserId list
  NewPostId : PostId
}

let newPostRequest userIds newPostsId =
  {UserIds = userIds; NewPostId = newPostsId}


let createPostAndNotifyFollowers (req : CreatePostRequest) = 
  //  Async<Result<UserId, Exception>>
  let getFollowersResult = getFollowersIds req.UserId
  // Async<Result<PostId, Exception>>
  let createPostResult = createPost req

  // Async<Result<NotifyNewPostRequest, Exception>>
  let newPostRequestResult =
    AsyncResult.map2 newPostRequest getFollowersResult createPostResult   

  // ...
```

