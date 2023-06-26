## CancellableTaskResult.mapError

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> CancellableTask<Result<'c, 'a>> -> CancellableTask<Result<'c, 'b>>
```

## Examples

### Example 1

From the [CancellableTaskResult.map](../cancellableTaskResult/map.md#example-1) example, if we want to map the error part alone, we can do it as below:

```fsharp
// CancellableTask<Result<PostId>, string>
createPost createPostRequest
|> CancellableTaskResult.mapError (fun (ex : exn) -> ex.Message)
```

