## CancellableTaskResult Infix Operators

Namespace: `FsToolkit.ErrorHandling.Operator.CancellableTaskResult`

FsToolkit.ErrorHandling provides the standard infix operators for the `map` (`<!>`), `apply` (`<*>`), and `bind` (`>>=`) functions of the `CancellableTask<Result<_,_>>` type.

## Examples:

### Example 1

Expanding on the [CancellableTaskResult.map2 example](../cancellableTaskResult/map2.md#example-1), we define another function:

```fsharp
notifyFollowers : NotifyNewPostRequest -> CancellableTask<Result<unit, exn>>
```

We can then rewrite the example and additionally call `notifyFollowers` using the operators as below:

```fsharp
open FsToolkit.ErrorHandling.Operator.CancellableTaskResult

// CreatePostRequest -> CancellableTask<Result<unit, exn>>
let createPostAndNotifyFollowers (req : CreatePostRequest) = 
  notifyNewPostRequest
  <!> (getFollowerIds req.UserId)
  <*> (createPost req)
  >>= notifyFollowers
```
