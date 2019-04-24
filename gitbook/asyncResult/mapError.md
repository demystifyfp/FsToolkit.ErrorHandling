## AsyncResult.mapError

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> Async<Result<'c, 'a>> -> Async<Result<'c, 'b>>
```

## Examples

### Example 1

From the [AsyncResult.map](../asyncResult/map.md#example-1) example, if we want to map the error part alone, we can do it as below:

```fsharp
// Async<Result<PostId>, string>
createPost createPostRequest
|> AsyncResult.mapError (fun (ex : exn) -> ex.Message)
```

