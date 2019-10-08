## JobResult.ignore

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Job<Result<'a, 'b>> -> Job<Result<unit, 'b>>
```

This is a shortcut for `JobResult.map ignore`.

## Examples

### Example 1

```fsharp
let savePost : CreatePostRequest -> Job<Result<PostId, exn>>
```

We can call this with the `do!` syntax inside a computation expression using `JobResult.ignore` as below:

```fsharp
let makePost = jobResult {
  do! savePost createPostRequest |> JobResult.ignore
}
```