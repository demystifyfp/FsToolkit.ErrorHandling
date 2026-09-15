## Task.ignore

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Task<'a> -> Task<unit>
```

This is a shortcut for `Task.map ignore`.

## Examples

### Example 1

```fsharp
let savePost : CreatePostRequest -> Task<PostId, exn>
```

We can call this with the `do!` syntax inside a computation expression using `Task.ignore` as below. Note the type being ignored must be specified to prevent accidental variances in return type varying intent.

```fsharp
let makePost = task {
  do! savePost createPostRequest |> Task.ignore<PostRequest>
}
```
