## AsyncResult.ignore

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Async<Result<'a, 'b>> -> Async<Result<unit, 'b>>
```

This is a shortcut for `AsyncResult.map ignore`.

## Examples

### Example 1

```fsharp
let savePost : CreatePostRequest -> Async<Result<PostId, exn>>
```

We can call this with the `do!` syntax inside a computation expression using `AsyncResult.ignore` as below.
Note the type being ignored may be pinned to prevent accidental variances in return type deviating from the code's intent.

```fsharp
let makePost = asyncResult {
  do! savePost createPostRequest |> AsyncResult.ignore<PostId, _>
}
```
