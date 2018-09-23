## AsyncResult Computation Expression

Namespace: `FsToolkit.ErrorHandling.CE.AsyncResult`


## Examples 

### Example 1

The [AsyncResult.bind example](../asyncResult/bind.md#example-1) can be written using computation expression as below

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

let notifyNewPostRequest userIds newPostsId =
  {UserIds = userIds; NewPostId = newPostsId}

// NotifyNewPostRequest -> Async<Result<Unit,Exception>>
let notifyFollowers (req : NotifyNewPostRequest) = async {
  // ...
}

// CreatePostRequest -> Async<Result<Unit,Exception>>
let createPostAndNotifyFollowers (req : CreatePostRequest) = async {
  let! followers = getFollowersIds req.UserId
  let! newPostId = createPost req
  return! notifyFollowers (notifyNewPostRequest followers newPostId)
}
```