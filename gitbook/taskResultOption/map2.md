## TaskResultOption.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c)
  -> Task<Result<'a option, 'd>>
  -> Task<Result<'b option, 'd>>
  -> Task<Result<'c option, 'd>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `taskResultOption` computation expression](ce.md).

### Example 1

Given the following functions:

```fsharp
getPostById : PostId -> Task<Result<Post option, exn>>
getUserById : UserId -> Task<Result<User option, exn>>
userTweet : Post -> User -> UserTweet
```

Then given a `PostId` and a `UserId`, we can call `userTweet` like this:

```fsharp
// Task<Result<UserTweet option, Exception>>
TaskResultOption.map2 userTweet (getPostById postId) (getUserById userId)
```

