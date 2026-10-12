## TaskResultOption.ignore

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Task<Result<'a option, 'b>> -> Task<Result<unit option, 'b>>
```

This is a shortcut for `TaskResultOption.map ignore`.

## Examples

### Example 1

```fsharp
let deletePostIfExists : DeletePostRequest -> Task<Result<Post option, exn>>
```

We can call this with the `do!` syntax inside a computation expression using `TaskResultOption.ignore` as below.
Note the type being ignored may be pinned to prevent accidental variances in return type deviating from the code's intent.

```fsharp
let deletePost = taskResultOption {
  do! deletePostIfExists deletePostRequest |> TaskResultOption.ignore<Post, _>
}
```
