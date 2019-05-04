## AsyncResult Infix Operators

Namespace: `FsToolkit.ErrorHandling.Operator.AsyncResult`

FsToolkit.ErrorHandling provides the standard infix operators for the `map` (`<!>`), `apply` (`<*>`), and `bind` (`>>=`) functions of the `Async<Result<_,_>>` type.

## Examples:

### Example 1

Expanding on the [AsyncResult.map2 example](../asyncResult/map2.md#example-1), we define another function:

```fsharp
notifyFollowers : NotifyNewPostRequest -> Async<Result<unit, exn>>
```

We can then rewrite the example and additionally call `notifyFollowers` using the operators as below:

```fsharp
open FsToolkit.ErrorHandling.Operator.AsyncResult

// CreatePostRequest -> Async<Result<unit, exn>>
let createPostAndNotifyFollowers (req : CreatePostRequest) = 
  notifyNewPostRequest
  <!> (getFollowerIds req.UserId)
  <*> (createPost req)
  >>= notifyFollowers
```
