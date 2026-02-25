## JobResultOption.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c -> 'd)
  -> Job<Result<'a option, 'e>>
  -> Job<Result<'b option, 'e>>
  -> Job<Result<'c option, 'e>>
  -> Job<Result<'d option, 'e>>
```

## Examples

Note: Many use-cases requiring `map3` can also be solved using [the `jobResultOption` computation expression](ce.md).

### Example 1

Given the following functions:

```fsharp
getUserById : UserId -> Job<Result<User option, exn>>
getPostById : PostId -> Job<Result<Post option, exn>>
getCommentById : CommentId -> Job<Result<Comment option, exn>>
```

And a combining function:

```fsharp
let combine user post comment = (user, post, comment)
```

We can combine all three using `JobResultOption.map3`:

```fsharp
// Job<Result<(User * Post * Comment) option, exn>>
JobResultOption.map3
  combine
  (getUserById sampleUserId)
  (getPostById samplePostId)
  (getCommentById sampleCommentId)
```
