## JobResultOption.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c)
  -> Job<Result<'a option, 'd>>
  -> Job<Result<'b option, 'd>>
  -> Job<Result<'c option, 'd>>
```

## Examples

Note: Many use-cases requiring `map2` can also be solved using [the `jobResultOption` computation expression](ce.md).

### Example 1

Given the following functions:

```fsharp
getUserById : UserId -> Job<Result<User option, exn>>
getPostById : PostId -> Job<Result<Post option, exn>>
```

And the type:

```fsharp
type UserTweet = { User: User; Post: Post }
let userTweet user post = { User = user; Post = post }
```

We can combine the results using `JobResultOption.map2` as below:

```fsharp
// Job<Result<UserTweet option, exn>>
JobResultOption.map2
  userTweet
  (getUserById sampleUserId)
  (getPostById samplePostId)
```
