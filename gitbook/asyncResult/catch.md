## AsyncResult.catch

Namespace: `FsToolkit.ErrorHandling`

Catches exceptions thrown by an async computation and maps them to the `Error` case using the provided function. If the async computation completes successfully with an `Ok` or `Error` value, that value is returned unchanged.

## Function Signature:

```fsharp
(exn -> 'error) -> Async<Result<'ok, 'error>> -> Async<Result<'ok, 'error>>
```

## Examples

### Example 1

Catching any exception and converting it to a string error:

```fsharp
let result : Async<Result<int, string>> =
  async { return failwith "something went wrong" }
  |> Async.map Ok
  |> AsyncResult.catch (fun ex -> ex.Message)
// evaluates to Error "something went wrong"
```

### Example 2

Wrapping a potentially throwing async call in an `AsyncResult`:

```fsharp
let safeFetch (url: string) : Async<Result<string, string>> =
  async {
    use client = new System.Net.Http.HttpClient()
    return! client.GetStringAsync(url) |> Async.AwaitTask |> Async.map Ok
  }
  |> AsyncResult.catch (fun ex -> sprintf "HTTP error: %s" ex.Message)
```

### Example 3

When no exception is thrown, the result passes through unchanged:

```fsharp
AsyncResult.ok 42
|> AsyncResult.catch (fun _ -> "error")
// evaluates to Ok 42
```
