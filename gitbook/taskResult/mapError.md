## TaskResult.mapError

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> Task<Result<'c, 'a>> -> Task<Result<'c, 'b>>
```

## Examples

### Example 1

From the [TaskResult.map](../taskResult/map.md#example-1) example, if we want to map the error part alone, we can do it as below:

```fsharp
// Task<Result<PostId>, string>
createPost createPostRequest
|> TaskResult.mapError (fun (ex : exn) -> ex.Message)
```

