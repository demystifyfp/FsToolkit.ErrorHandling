## TaskResult.either

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> ('c -> 'b) -> Task<Result<'a, 'c>> -> Task<'b>
```

This is just a shortcut for `Task.map Result.either`. See [Result.either](../result/either.md) for more.

## Examples

### Example 1

```fsharp
type HttpResponse<'a, 'b> =
  | Ok of 'a
  | InternalError of 'b

// CreatePostRequest -> Task<Result<PostId, exn>>
let createPost (req : CreatePostRequest) = task {
  // ...
}

// Task<HttpResponse<PostId, exn>>
let handler (httpReq : HttpRequest) = 
  // ... 
  
  // Task<Result<PostId, exn>>
  let createPostTR = createPost httpReq

  createPostTR
  |> TaskResult.either Ok InternalError
```
