## CancellableTaskResult.either

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> ('c -> 'b) -> CancellableTask<Result<'a, 'c>> -> Task<'b>
```

This is just a shortcut for `CancellableTask.map Result.either`. See [Result.either](../result/eitherFunctions.md) for more.

## Examples

### Example 1

```fsharp
type HttpResponse<'a, 'b> =
  | Ok of 'a
  | InternalError of 'b

// CreatePostRequest -> CancellableTask<Result<PostId, exn>>
let createPost (req : CreatePostRequest) = cancellableTask {
  // ...
}

// CancellableTask<HttpResponse<_, _>>
let handler (httpReq : HttpRequest) = 
  // ... 
  
  // CancellableTask<Result<PostId, exn>>
  let createPostTR = createPost httpReq

  createPostTR
  |> CancellableTaskResult.either Ok InternalError
```
