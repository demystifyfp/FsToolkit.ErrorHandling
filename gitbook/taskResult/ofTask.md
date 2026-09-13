# TaskResult.ofTask

Namespace: `FsToolkit.ErrorHandling`

Transforms a `Task<'T>` into a `Task<Result<'T, exn>>` by wrapping the value in `Ok`.

> **Note:** This function does **not** catch exceptions thrown by the task. Any exceptions will propagate as-is. To catch exceptions and map them to the `Error` case, use [`catchWith`](catchWith.md).

## Function Signature

```fsharp
Task<'T> -> Task<Result<'T, exn>>
```

## Examples

### Example 1

```fsharp
let result =
    task { return 42 }
    |> TaskResult.ofTask
// task { return Ok 42 }
```

### Example 2

```fsharp
// Exceptions are NOT caught — they propagate out of the task
let result =
    task { return failwith "something went wrong" }
    |> TaskResult.ofTask
// throws System.Exception("something went wrong")
```

