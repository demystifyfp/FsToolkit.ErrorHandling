# Result.ignore

Namespace: `FsToolkit.ErrorHandling`

This is a shortcut for `Result.map ignore`.

## Function Signature

```fsharp
Result<'a, 'b> -> Result<unit, 'b>
```

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
