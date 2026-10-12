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

We can call this with the `do!` syntax inside a computation expression using `TaskResult.ignore` as below. 
Note the type being ignored may be pinned to prevent accidental variances in return type deviating from the code's intent.

```fsharp
let makePost = taskResult {
    do! savePost createPostRequest |> TaskResult.ignore<PostId, _>
}
```
