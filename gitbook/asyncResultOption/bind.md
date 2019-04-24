## AsyncResultOption.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Async<Result<'b option, 'c>>)
  -> Async<Result<'a option, 'c>>
  -> Async<Result<'b option, 'c>>
```

## Examples

Note: Many use-cases requiring `bind` can also be solved using [the `asyncResultOption` computation expression](ce.md).

### Example 1

Given the following functions:

```fsharp
getUserById : UserId -> Async<Result<User option, exn>>
getPostById : PostId -> Async<Result<Post option, exn>>
```

We can get a post's user given a `PostId` like this:

```fsharp
// Async<Result<Post option, exn>>
getPostById postId
|> AsyncResultOption.bind (fun post -> getUserById post.UserId)
```