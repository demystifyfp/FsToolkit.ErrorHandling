## AsyncResultOption.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> Async<Result<'b option, 'c>>) -> Async<Result<'a option, 'c>> 
  -> Async<Result<'b option, 'c>>
```

## Examples

### Example 1

```fsharp
// UserId -> Async<Result<User option, Exception>>
let getUserById (userId : UserId) = async {
  // ...
}

// PostId -> Async<Result<Post option, Exception>>
let getPostById (postId : PostId) = async {
  // ...
}

// Async<Result<Post option, Exception>>
getPostById samplePostId
|> AsyncResultOption.bind (fun post -> getUserById post.UserId)
```