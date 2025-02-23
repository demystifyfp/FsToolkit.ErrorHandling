## TaskResult.catch

Namespace: `FsToolkit.ErrorHandling`

Catches exceptions and maps them to the Error case using the provided function.

Function Signature:

```fsharp
(exn -> 'a) -> Task<Result<'b, 'a>> -> Task<Result<'b, 'a>>
```

## Examples

Given the function:

```fsharp
let taskThrow () =
  task {
    failwith "an exception happened"
    return Error ""
  }
```

### Example 1

```fsharp
let result : Task<Result<int, string>> = TaskResult.catch (_.Message) (TaskResult.ok 42)
// task { Ok 42 }
```

### Example 2

```fsharp
let result : Task<Result<int, string>> = TaskResult.catch (_.Message) (TaskResult.error "something bad happened")
// task { Error "something bad happened" }
```

### Example 3

```fsharp
let result : Task<Result<int, string>> = TaskResult.catch (_.Message) (taskThrow ())
// task { Error "an exception happened" }
```

