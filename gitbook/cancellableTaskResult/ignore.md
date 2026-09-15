## CancellableTaskResult.ignore

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
CancellableTask<Result<'a, 'b>> -> CancellableTask<Result<unit, 'b>>
```

This is a shortcut for `CancellableTaskResult.map ignore`.

## Examples

### Example 1

```fsharp
let savePost : CreatePostRequest -> CancellableTask<Result<PostId, exn>>
```

We can call this with the `do!` syntax inside a computation expression using `CancellableTaskResult.ignore` as below.
Note the type being ignored may be pinned to prevent accidental variances in return type deviating from the code's intent.

```fsharp
let makePost = cancellableTaskResult {
  do! savePost createPostRequest |> CancellableTaskResult.ignore<PostId, _>
}
```
