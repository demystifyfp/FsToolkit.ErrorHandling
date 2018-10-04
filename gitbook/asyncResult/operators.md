## AsyncResult Infix Operators

FsToolkit.ErrorHandling provides the standard infix operators for the map (`<!>`), apply (`<*>`) & bind (`>>=`) functions.

Namespace: `FsToolkit.ErrorHandling.Operator.AsyncResult`

## Examples:

### Example 1

The [AsyncResult Computation Expression example](../asyncResult/ce.md#example-1) can be written using operators as below

```fsharp
open FsToolkit.ErrorHandling.Operator.AsyncResult

// CreatePostRequest -> Async<Result<Unit,Exception>>
let createPostAndNotifyFollowers (req : CreatePostRequest) = 
  notifyNewPostRequest 
  <!> (getFollowersIds req.UserId) 
  <*> (createPost req)
  >>= notifyFollowers
```