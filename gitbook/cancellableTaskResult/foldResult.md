## CancellableTaskResult.foldResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> ('c -> 'b) -> CancellableTask<Result<'a, 'c>> -> Task<'b>
```

This is just a shortcut for `Task.map Result.fold`. See [Result.fold](../result/fold.md) for more.

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

// CancellableTask<HttpResponse>
let handler (httpReq : HttpRequest) = 
  // ... 
  
  // CancellableTask<Result<PostId, exn>>
  let createPostTR = createPost httpReq

  createPostTR
  |> CancellableTaskResult.fold Ok InternalError
```
