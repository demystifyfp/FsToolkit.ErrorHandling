## JobResult Infix Operators

Namespace: `FsToolkit.ErrorHandling.Operator.JobResult`

FsToolkit.ErrorHandling provides the standard infix operators for the `map` (`<!>`), `apply` (`<*>`), and `bind` (`>>=`) functions of the `Job<Result<_,_>>` type.

## Examples:

### Example 1

Expanding on the [JobResult.map2 example](../jobResult/map2.md#example-1), we define another function:

```fsharp
notifyFollowers : NotifyNewPostRequest -> Job<Result<unit, exn>>
```

We can then rewrite the example and additionally call `notifyFollowers` using the operators as below:

```fsharp
open FsToolkit.ErrorHandling.Operator.JobResult

// CreatePostRequest -> Job<Result<unit, exn>>
let createPostAndNotifyFollowers (req : CreatePostRequest) = 
  notifyNewPostRequest
  <!> (getFollowerIds req.UserId)
  <*> (createPost req)
  >>= notifyFollowers
```
