## JobResult.zipError

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Job<Result<'a, 'b>> -> Job<Result<'a, 'c>> -> Job<Result<'a, ('b * 'c)>>
```

Takes two job-wrapped results and returns a job-wrapped result where the error is a tuple of both errors. If either input is `Ok`, returns that `Ok`.

## Examples

### Example 1

```fsharp
let jobErr1 = JobResult.error "error1"
let jobErr2 = JobResult.error "error2"

JobResult.zipError jobErr1 jobErr2
// job { return Error ("error1", "error2") }
```

### Example 2

```fsharp
let jobOk = JobResult.singleton 42
let jobErr = JobResult.error "something went wrong"

JobResult.zipError jobOk jobErr
// job { return Ok 42 }
```

### Example 3

```fsharp
let validateAge : Job<Result<int, string>>
let validateName : Job<Result<string, string>>

// Job<Result<int, (string * string)>>
JobResult.zipError validateAge validateName
```
