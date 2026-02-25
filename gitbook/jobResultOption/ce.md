## JobResultOption Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples

### Example 1

Given the following functions:

```fsharp
getUserById : UserId -> Job<Result<User option, exn>>
getPostById : PostId -> Job<Result<Post option, exn>>
```

We can compose them using the `jobResultOption` computation expression:

```fsharp
// Job<Result<(User * Post) option, exn>>
jobResultOption {
  let! post = getPostById samplePostId
  let! user = getUserById post.UserId
  return (user, post)
}
```

### Example 2

```fsharp
tryFindUser : string -> Job<Result<User option, string>>
tryFindPost : string -> Job<Result<Post option, string>>

// Job<Result<string option, string>>
let result = jobResultOption {
  let! user = tryFindUser "alice"
  let! post = tryFindPost "my-post"
  return sprintf "%s wrote: %s" user.Name post.Title
}
```
