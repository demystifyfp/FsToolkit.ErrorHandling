# JobResult.fromTask

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Task` into a `Job<Result<unit, exn>>`

## Function Signature

```fsharp
Task -> Job<Result<unit, exn>>
```

## Examples

### Example 1

```fsharp
let result = JobResult.fromUnitTask (task { return () })
// job { return Ok () }
```

### Example 2

```fsharp
let result = JobResult.fromUnitTask (task { return System.Exception("error") })
// job { return Error "error" }
```
