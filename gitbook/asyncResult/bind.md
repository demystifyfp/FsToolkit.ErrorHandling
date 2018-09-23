## ResultOption.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> Async<Result<'b, 'c>>) -> Async<Result<'a, 'c>> 
  -> Async<Result<'b, 'c>>
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
  // ...
}

let newPostRequest userIds newPostsId =
  {UserIds = userIds; NewPostId = newPostsId}

// NotifyNewPostRequest -> Async<Result<Unit,Exception>>
let notifyFollowers (req : NotifyNewPostRequest) = async {
  // ...
}

// CreatePostRequest -> Async<Result<Unit,Exception>>
let createPostAndNotifyFollowers (req : CreatePostRequest) = 
  //  Async<Result<UserId, Exception>>
  let getFollowersResult = getFollowersIds req.UserId
  // Async<Result<PostId, Exception>>
  let createPostResult = createPost req

  AsyncResult.map2 newPostRequest getFollowersResult createPostResult
  |> AsyncResult.bind notifyFollowers
```