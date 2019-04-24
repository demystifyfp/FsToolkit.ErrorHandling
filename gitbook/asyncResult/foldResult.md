## AsyncResult.foldResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> ('c -> 'b) -> Async<Result<'a, 'c>> -> Async<'b>
```

This is just a shortcut for `Async.map Result.fold`. See [Result.fold](../result/fold.md) for more.

## Examples

### Example 1

```fsharp
type HttpResponse<'a, 'b> =
  | OK of 'a
  | InternalError of 'b

// CreatePostRequest -> Async<Result<PostId, exn>>
let createPost (req : CreatePostRequest) = async {
  // ...
}

// Async<HttpResponse>
let handler (httpReq : HttpRequest) = 
  // ... 
  
  // Async<Result<PostId, exn>>
  let createPostAR = createPost httpReq

  createPostAR
  |> AsyncResult.fold Ok InternalError
```
