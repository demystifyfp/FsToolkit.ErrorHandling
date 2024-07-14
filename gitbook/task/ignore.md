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

We can call this with the `do!` syntax inside a computation expression using `Task.ignore` as below:

```fsharp
let makePost = task {
  do! savePost createPostRequest |> Task.ignore
}
```
