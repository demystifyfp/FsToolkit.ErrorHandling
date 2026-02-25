## JobResultOption.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Job<Result<'b option, 'c>>)
  -> Job<Result<'a option, 'c>>
  -> Job<Result<'b option, 'c>>
```

## Examples

Note: Many use-cases requiring `bind` can also be solved using [the `jobResultOption` computation expression](ce.md).

### Example 1

Given the following functions:

```fsharp
getUserById : UserId -> Job<Result<User option, exn>>
getPostById : PostId -> Job<Result<Post option, exn>>
```

We can get a post's user given a `PostId` like this:

```fsharp
// Job<Result<User option, exn>>
getPostById postId
|> JobResultOption.bind (fun post -> getUserById post.UserId)
```

### Example 2

```fsharp
tryFindConfig : string -> Job<Result<Config option, string>>
tryLoadData : Config -> Job<Result<Data option, string>>

// Job<Result<Data option, string>>
tryFindConfig "app.json"
|> JobResultOption.bind tryLoadData
```
