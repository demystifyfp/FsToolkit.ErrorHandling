# Job.map2

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b -> 'c) -> Job<'a> -> Job<'b> -> Job<'c>
```

## Examples

### Example 1

Given the functions

```fsharp
getFollowerIds : UserId -> Job<UserId list>
createPost : CreatePostRequest -> Job<PostId>
```

And the type

```fsharp
type NotifyNewPostRequest =
  { UserIds : UserId list
    NewPostId : PostId }
  static member Create userIds newPostId =
    { UserIds = userIds; NewPostId = newPostId }
```

We can create a `NotifyNewPostRequest` using `Job.map2` as below:

```fsharp
let createPostAndGetNotifyRequest (req : CreatePostRequest) =
  // Job<NotifyNewPostRequest>
  Job.map2
    NotifyNewPostRequest.Create
    (getFollowerIds req.UserId)
    (createPost req)
```
