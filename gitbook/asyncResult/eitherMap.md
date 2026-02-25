## AsyncResult.eitherMap

Namespace: `FsToolkit.ErrorHandling`

Applies one of two mapping functions to an `Async<Result<'okInput, 'errorInput>>`: the first function is applied if the result is `Ok`, and the second is applied if the result is `Error`.

## Function Signature:

```fsharp
('okInput -> 'okOutput)
  -> ('errorInput -> 'errorOutput)
  -> Async<Result<'okInput, 'errorInput>>
  -> Async<Result<'okOutput, 'errorOutput>>
```

## Examples

### Example 1

Transforming both the Ok and Error branches of an async result:

```fsharp
let result : Async<Result<string, int>> =
  AsyncResult.ok 42
  |> AsyncResult.eitherMap
       (fun n -> sprintf "Got %d" n)
       (fun err -> -1)
// evaluates to Ok "Got 42"
```

### Example 2

Mapping an Error value while the Ok path is unchanged in type:

```fsharp
let result : Async<Result<string, string>> =
  AsyncResult.error 404
  |> AsyncResult.eitherMap
       (fun s -> s.ToUpper())
       (fun code -> sprintf "Error code: %d" code)
// evaluates to Error "Error code: 404"
```

### Example 3

Normalising both branches to the same type before consuming:

```fsharp
let displayMessage : Async<string> =
  fetchData ()
  |> AsyncResult.eitherMap
       (fun data -> sprintf "Success: %s" data)
       (fun err  -> sprintf "Failed: %s" err)
  |> AsyncResult.foldResult id id
```
