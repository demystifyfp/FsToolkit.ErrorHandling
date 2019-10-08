## Result.ignore

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Result<'a, 'b> -> Result<unit, 'b>
```

This is a shortcut for `Result.map ignore`.

## Examples

### Example 1

```fsharp
let savePost : CreatePostRequest -> Result<PostId, exn>
```

We can call this with the `do!` syntax inside a computation expression using `Result.ignore` as below:

```fsharp
let makePost = result {
  do! savePost createPostRequest |> Result.ignore
}
```