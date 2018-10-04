## AsyncResultOption Infix Operators

FsToolkit.ErrorHandling provides the standard infix operators for the map (`<!>`), apply (`<*>`) & bind (`>>=`) functions.

Namespace: `FsToolkit.ErrorHandling.Operator.AsyncResultOption`

## Examples:

### Example 1

The [AsyncResultOption map2 example](../asyncResultOption/map2.md#example-1) can be written using operators as below

```fsharp
open FsToolkit.ErrorHandling.Operator.AsyncResult

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

// Async<Result<UserTweet option, Exception>>
userTweet 
<!> (getPostById samplePostId) 
<*> (getUserById sampleUserId)
```