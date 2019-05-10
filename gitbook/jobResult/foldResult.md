## JobResult.foldResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> ('c -> 'b) -> Job<Result<'a, 'c>> -> Job<'b>
```

This is just a shortcut for `Job.map Result.fold`. See [Result.fold](../result/fold.md) for more.

## Examples

### Example 1

```fsharp
type HttpResponse<'a, 'b> =
  | OK of 'a
  | InternalError of 'b

// CreatePostRequest -> Job<Result<PostId, exn>>
let createPost (req : CreatePostRequest) = job {
  // ...
}

// Job<HttpResponse>
let handler (httpReq : HttpRequest) = 
  // ... 
  
  // Job<Result<PostId, exn>>
  let createPostAR = createPost httpReq

  createPostAR
  |> JobResult.fold Ok InternalError
```
