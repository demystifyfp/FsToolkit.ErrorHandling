## AsyncResultOption Computation Expression

Namespace: `FsToolkit.ErrorHandling`


## Examples 

### Example 1

The [AsyncResultOption.map2 example](../asyncResultOption/map2.md#example-1) can be written using the `asyncResultOption` computation expression as below:

```fsharp
// Async<Result<UserTweet option, exn>>
asyncResultOption {
  let! post = getPostById samplePostId
  let! user = getUserById post.UserId
  return userTweet post user
}
```
