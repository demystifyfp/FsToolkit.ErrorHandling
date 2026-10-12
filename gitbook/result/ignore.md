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

We can call this with the `do!` syntax inside a computation expression using `Result.ignore` as below. 
Note the type being ignored may be pinned to prevent accidental variances in return type deviating from the code's intent.

```fsharp
let makePost = result {
    do! savePost createPostRequest |> Result.ignore<PostId, _>
}
```
