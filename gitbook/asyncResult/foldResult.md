## Result.foldResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b) -> ('c -> 'b) -> Async<Result<'a, 'c>> -> Async<'b>
```

## Examples

### Example 1

```fsharp
type HttpResponse<'a, 'b> =
  | OK of 'a
  | InternalError of 'b

// CreatePostRequest -> Async<Result<PostId, Exception>>
let createPost (req : CreatePostRequest) = async {
  // ...
}    

// Async<HttpResponse>
let handler (httpReq : HttpRequest) = 
  // ... 
  
  // Async<Result<PostId, Exception>>
  let createPostAR = createPost httpReq

  createPostAR
  |> AsyncResult.fold Ok InternalError
```
