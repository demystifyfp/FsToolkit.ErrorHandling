## AsyncResult.mapError

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b) -> Async<Result<'c, 'a>> 
  -> Async<Result<'c, 'b>>
```

## Examples

### Example 1

From the example [AsyncResult.map](../asyncResult/map.md#example-1), if we want to the error part alone, we can do it as below

```fsharp
// Async<Result<PostId>, string>
createPost createPostRequest
|> AsyncResult.mapError (fun (ex : Exception) -> ex.Message)
```