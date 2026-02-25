# TaskResult.ofCatchTask

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Task<'T>` into a `Task<Result<'T, exn>>`, catching any exceptions thrown by the task and wrapping them in the `Error` case.

Unlike [`ofTask`](ofTask.md), this function catches exceptions that escape from the task and maps them to `Error`. If the task completes successfully, the result is wrapped in `Ok`.

## Function Signature

```fsharp
Task<'T> -> Task<Result<'T, exn>>
```

## Examples

### Example 1

```fsharp
let result = TaskResult.ofCatchTask (task { return 42 })
// task { return Ok 42 }
```

### Example 2

```fsharp
let result = TaskResult.ofCatchTask (task { return failwith "something went wrong" })
// task { return Error (System.Exception("something went wrong")) }
```
