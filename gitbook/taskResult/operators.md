## TaskResult Infix Operators

Namespace: `FsToolkit.ErrorHandling.Operator.TaskResult`

FsToolkit.ErrorHandling provides the standard infix operators for the `map` (`<!>`), `apply` (`<*>`), and `bind` (`>>=`) functions of the `Task<Result<_,_>>` type.

## Examples:

### Example 1

Expanding on the [TaskResult.map2 example](../taskResult/map2.md#example-1), we define another function:

```fsharp
notifyFollowers : NotifyNewPostRequest -> Task<Result<unit, exn>>
```

We can then rewrite the example and additionally call `notifyFollowers` using the operators as below:

```fsharp
open FsToolkit.ErrorHandling.Operator.TaskResult

// CreatePostRequest -> Task<Result<unit, exn>>
let createPostAndNotifyFollowers (req : CreatePostRequest) = 
  notifyNewPostRequest
  <!> (getFollowerIds req.UserId)
  <*> (createPost req)
  >>= notifyFollowers
```
