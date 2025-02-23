## Task.catch

Namespace: `FsToolkit.ErrorHandling`

Creates a `Task` that attempts to execute the provided task, returning `Choice1Of2` with the result if the task completes without exceptions, or `Choice2Of2` with the exception if an exception is thrown.

Function Signature:

```fsharp
Task<'a> -> Task<Choice<'a, exn>>
```

## Examples

Given the function:

```fsharp
let taskThrow () =
  task {
    failwith "something bad happened"
    return Error ""
  }
```

### Example 1

```fsharp
let result = Task.catch (Task.singleton 42)
// task { Choice1Of2(42) }
```

### Example 2

Given the function

```fsharp
let result = Task.catch (taskThrow ())
// task { Choice2Of2(exn("something bad happened")) }
```

