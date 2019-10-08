## ResultOption.ignore

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Result<'a option, 'b> -> Result<unit option, 'b>
```

This is a shortcut for `ResultOption.map ignore`.

## Examples

### Example 1

```fsharp
let deletePostIfExists : DeletePostRequest -> Result<Post option, exn>
```

We can call this with the `do!` syntax inside a computation expression using `ResultOption.ignore` as below:

```fsharp
let deletePost = resultOption {
  do! deletePostIfExists deletePostRequest |> ResultOption.ignore
}
```