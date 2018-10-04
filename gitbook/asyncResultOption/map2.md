## AsyncResultOption.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> 'b -> 'c) -> Async<Result<'a option, 'd>> -> Async<Result<'b option, 'd>> 
  -> Async<Result<'c option, 'd>>
```

## Examples

### Example 1

```fsharp
// Post -> User -> UserTweet
let userTweet (p : Post) (u : User) =
  // ...

// UserId -> Async<Result<User option, Exception>>
let getUserById (userId : UserId) = async {
  // ...
}

// PostId -> Async<Result<Post option, Exception>>
let getPostById (postId : PostId) = async {
  // ...
}

// Async<Result<UserTweet option, Exception>>
AsyncResultOption.map2 userTweet (getPostById postId) (getUserById userId)
```