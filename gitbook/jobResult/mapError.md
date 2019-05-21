## JobResult.mapError

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> Job<Result<'c, 'a>> -> Job<Result<'c, 'b>>
```

## Examples

### Example 1

From the [JobResult.map](../jobResult/map.md#example-1) example, if we want to map the error part alone, we can do it as below:

```fsharp
// Job<Result<PostId>, string>
createPost createPostRequest
|> JobResult.mapError (fun (ex : exn) -> ex.Message)
```

