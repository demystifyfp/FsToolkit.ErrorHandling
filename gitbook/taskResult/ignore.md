## TaskResult.ignore

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Task<Result<'a, 'b>> -> Task<Result<unit, 'b>>
```

This is a shortcut for `TaskResult.map ignore`.

## Examples

### Example 1

```fsharp
let savePost : CreatePostRequest -> Task<Result<PostId, exn>>
```

We can call this with the `do!` syntax inside a computation expression using `TaskResult.ignore` as below:

```fsharp
let makePost = taskResult {
  do! savePost createPostRequest |> TaskResult.ignore
}
```