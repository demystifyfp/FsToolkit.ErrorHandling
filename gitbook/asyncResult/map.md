## AsyncResult.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b) -> Async<Result<'a, 'c>> 
  -> Async<Result<'b, 'c>>
```

## Examples

### Example 1

As a continuation of [Result.map3 (Example 2)](../result/map3#example-2), let's assume that we want to store the created post in the database (using a fake function `createPost`)

```fsharp
// A fake function to represent the DB call

// CreatePostRequest -> Async<Result<PostId, Exception>>
let createPost (req : CreatePostRequest) = async {
  let (UserId userId) = req.UserId
  
  if ... then
    return Error (new Exception("something went wrong"))
  else
    return Ok samplePostId
}
```

To return the id of the post, using the `AsyncResult.map` we can achieve the following

```fsharp
// Async<Result<Guid>, Exception>
createPost createPostRequest
|> AsyncResult.map (fun (PostId postId) -> postId)
```