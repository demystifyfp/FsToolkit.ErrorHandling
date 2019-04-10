## AsyncResultOption Computation Expression

Namespace: `FsToolkit.ErrorHandling`


## Examples 

### Example 1

The [AsyncResultOption.bind example](../asyncResultOption/bind.md#example-1) can be written using computation expression as below

```fsharp
// Post -> User -> UserTweet
let userTweet (p : Post) (u : User) =
  // ...

// UserId -> Async<Result<User option, Exception>>
let getUserById (userId : UserId) = async {
  // ...
}

// PostId -> Async<Result<Post option, Exception>>
let getPostById (postId : PostId) = async {
  // ...
}

// Async<Result<Post option, Exception>>
asyncResultOption {
  let! post = getPostById samplePostId
  let! user = getUserById post.UserId
  return userTweet post user
}
```