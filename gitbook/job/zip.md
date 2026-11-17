# Job.zip

Namespace: `FsToolkit.ErrorHandling`

Takes two jobs and returns a tuple of the pair.

## Function Signature

```fsharp
Job<'left> -> Job<'right> -> Job<('left * 'right)>
```

## Examples

### Example 1

```fsharp
let left = Job.result 123
let right = Job.result "abc"

Job.zip left right
// job { return (123, "abc") }
```

### Example 2

```fsharp
let fetchUser : UserId -> Job<User>
let fetchPost : PostId -> Job<Post>

// Job<(User * Post)>
Job.zip (fetchUser userId) (fetchPost postId)
```
