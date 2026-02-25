## JobResult.catch

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
(exn -> 'b) -> Job<Result<'a, 'b>> -> Job<Result<'a, 'b>>
```

Catches exceptions that occur during the execution of a `Job<Result<'a, 'b>>` and maps them to the `Error` case using the provided function.

## Examples

### Example 1

```fsharp
let riskyJob : Job<Result<int, string>> =
    job { return failwith "unexpected failure" }

riskyJob
|> JobResult.catch (fun ex -> ex.Message)
// job { return Error "unexpected failure" }
```

### Example 2

```fsharp
let safeDivide (x: int) (y: int) : Job<Result<int, string>> =
    job { return Ok (x / y) }

safeDivide 10 0
|> JobResult.catch (fun ex -> sprintf "Division error: %s" ex.Message)
// job { return Error "Division error: Attempted to divide by zero." }
```

### Example 3

```fsharp
let fetchData : Job<Result<string, AppError>> = // ...

fetchData
|> JobResult.catch (fun ex -> AppError.Unexpected ex)
// Exceptions become Error(AppError.Unexpected ex)
// Existing errors pass through unchanged
```
