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

We can call this with the `do!` syntax inside a computation expression using `TaskResultOption.ignore` as below:

```fsharp
let deletePost = taskResultOption {
  do! deletePostIfExists deletePostRequest |> TaskResultOption.ignore
}
```
