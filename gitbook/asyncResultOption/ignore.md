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

We can call this with the `do!` syntax inside a computation expression using `AsyncResultOption.ignore` as below:

```fsharp
let deletePost = asyncResultOption {
  do! deletePostIfExists deletePostRequest |> AsyncResultOption.ignore
}
```