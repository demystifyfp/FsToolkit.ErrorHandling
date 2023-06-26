## TaskResultOption Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples 

### Example 1

The [TaskResultOption.map2 example](../taskResultOption/map2.md#example-1) can be written using the `taskResultOption` computation expression as below:

```fsharp
// Task<Result<UserTweet option, exn>>
taskResultOption {
  let! post = getPostById samplePostId
  let! user = getUserById post.UserId
  return userTweet post user
}
```
