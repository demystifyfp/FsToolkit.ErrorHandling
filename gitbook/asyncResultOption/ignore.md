## AsyncResultOption.ignore

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Async<Result<'a option, 'b>> -> Async<Result<unit option, 'b>>
```

This is a shortcut for `AsyncResultOption.map ignore`.

## Examples

### Example 1

```fsharp
let deletePostIfExists : DeletePostRequest -> Async<Result<Post option, exn>>
```

We can call this with the `do!` syntax inside a computation expression using `AsyncResultOption.ignore` as below.
Note the type being ignored may be pinned to prevent accidental variances in return type deviating from the code's intent.

```fsharp
let deletePost = asyncResultOption {
  do! deletePostIfExists deletePostRequest |> AsyncResultOption.ignore<Post, _>
}
```
