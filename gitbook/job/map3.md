# Job.map3

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c -> 'd) -> Job<'a> -> Job<'b> -> Job<'c> -> Job<'d>
```

## Examples

### Example 1

Given the functions

```fsharp
getFollowerIds : UserId -> Job<UserId list>
createPost : CreatePostRequest -> Job<PostId>
getTimestamp : unit -> Job<DateTimeOffset>
```

And a combining function:

```fsharp
let makeNotification userIds postId timestamp =
  { UserIds = userIds; PostId = postId; CreatedAt = timestamp }
```

We can combine three jobs using `Job.map3` as below:

```fsharp
// Job<Notification>
Job.map3
  makeNotification
  (getFollowerIds req.UserId)
  (createPost req)
  (getTimestamp ())
```
