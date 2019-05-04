## TaskResult.foldResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> ('c -> 'b) -> Task<Result<'a, 'c>> -> Task<'b>
```

This is just a shortcut for `Task.map Result.fold`. See [Result.fold](../result/fold.md) for more.

## Examples

### Example 1

```fsharp
type HttpResponse<'a, 'b> =
  | OK of 'a
  | InternalError of 'b

// CreatePostRequest -> Task<Result<PostId, exn>>
let createPost (req : CreatePostRequest) = task {
  // ...
}

// Task<HttpResponse>
let handler (httpReq : HttpRequest) = 
  // ... 
  
  // Task<Result<PostId, exn>>
  let createPostAR = createPost httpReq

  createPostAR
  |> TaskResult.fold Ok InternalError
```
