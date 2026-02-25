## JobResult.zip

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Job<Result<'a, 'c>> -> Job<Result<'b, 'c>> -> Job<Result<('a * 'b), 'c>>
```

Takes two job-wrapped results and returns a job-wrapped tuple result. If either input is `Error`, returns that `Error`.

## Examples

### Example 1

```fsharp
let jobOk1 = JobResult.singleton 1
let jobOk2 = JobResult.singleton "hello"

JobResult.zip jobOk1 jobOk2
// job { return Ok (1, "hello") }
```

### Example 2

```fsharp
let jobOk = JobResult.singleton 42
let jobErr = JobResult.error "something went wrong"

JobResult.zip jobOk jobErr
// job { return Error "something went wrong" }
```

### Example 3

```fsharp
let getUser : UserId -> Job<Result<User, string>>
let getPost : PostId -> Job<Result<Post, string>>

// Job<Result<(User * Post), string>>
JobResult.zip (getUser userId) (getPost postId)
```
