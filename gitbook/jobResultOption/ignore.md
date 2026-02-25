## JobResultOption.ignore

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Job<Result<'a option, 'b>> -> Job<Result<unit option, 'b>>
```

This is a shortcut for `JobResultOption.map ignore`.

## Examples

### Example 1

```fsharp
let deletePostIfExists : DeletePostRequest -> Job<Result<Post option, exn>>
```

We can call this with the `do!` syntax inside a computation expression using `JobResultOption.ignore` as below:

```fsharp
let deletePost = jobResultOption {
  do! deletePostIfExists deletePostRequest |> JobResultOption.ignore
}
```
