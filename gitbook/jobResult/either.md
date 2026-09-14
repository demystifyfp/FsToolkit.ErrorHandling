## JobResult.either

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> ('c -> 'b) -> Job<Result<'a, 'c>> -> Job<'b>
```

This is just a shortcut for `Job.map Result.either`. See [Result.either](../result/either.md) for more.

## Examples

### Example 1

```fsharp
type HttpResponse<'a, 'b> =
  | Ok of 'a
  | InternalError of 'b

// CreatePostRequest -> Job<Result<PostId, exn>>
let createPost (req : CreatePostRequest) = job {
  // ...
}

// Job<HttpResponse>
let handler (httpReq : HttpRequest) = 
  // ... 
  
  // Job<Result<PostId, exn>>
  let createPostJR = createPost httpReq

  createPostJR
  |> JobResult.either Ok InternalError
```
