## AsyncResultOption.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c)
  -> Async<Result<'a option, 'd>>
  -> Async<Result<'b option, 'd>>
  -> Async<Result<'c option, 'd>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `asyncResultOption` computation expression](ce.md).

### Example 1

Given the following functions:

```fsharp
getPostById : PostId -> Async<Result<Post option, exn>>
getUserById : UserId -> Async<Result<User option, exn>>
userTweet : Post -> User -> UserTweet
```

Then given a `PostId` and a `UserId`, we can call `userTweet` like this:

```fsharp
// Async<Result<UserTweet option, Exception>>
AsyncResultOption.map2 userTweet (getPostById postId) (getUserById userId)
```

