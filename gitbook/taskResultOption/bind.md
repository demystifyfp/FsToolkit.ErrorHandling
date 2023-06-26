## TaskResultOption.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Task<Result<'b option, 'c>>)
  -> Task<Result<'a option, 'c>>
  -> Task<Result<'b option, 'c>>
```

## Examples

Note: Many use-cases requiring `bind` can also be solved using [the `taskResultOption` computation expression](ce.md).

### Example 1

Given the following functions:

```fsharp
getUserById : UserId -> Task<Result<User option, exn>>
getPostById : PostId -> Task<Result<Post option, exn>>
```

We can get a post's user given a `PostId` like this:

```fsharp
// Task<Result<Post option, exn>>
getPostById postId
|> TaskResultOption.bind (fun post -> getUserById post.UserId)
```
