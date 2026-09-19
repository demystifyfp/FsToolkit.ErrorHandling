## AsyncResult.either

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> ('c -> 'b) -> Async<Result<'a, 'c>> -> Async<'b>
```

This is just a shortcut for `Async.map Result.either`. See [Result.either](../result/eitherFunctions.md) for more.

## Examples

### Example 1

```fsharp
type HttpResponse<'a, 'b> =
  | Ok of 'a
  | InternalError of 'b

// CreatePostRequest -> Async<Result<PostId, exn>>
let createPost (req : CreatePostRequest) = async {
  // ...
}

// Async<HttpResponse<PostId, exn>>
let handler (httpReq : HttpRequest) = 
  // ... 
  
  // Async<Result<PostId, exn>>
  let createPostAR = createPost httpReq

  createPostAR
  |> AsyncResult.either Ok InternalError
```
