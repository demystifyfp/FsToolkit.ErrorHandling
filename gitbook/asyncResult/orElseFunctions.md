## AsyncResult.orElse

Namespace: `FsToolkit.ErrorHandling`

Returns the original result unchanged if it is `Ok`, otherwise returns the provided alternative result.

## Function Signature:

```fsharp
Async<Result<'ok, 'error2>>
  -> Async<Result<'ok, 'error>>
  -> Async<Result<'ok, 'error2>>
```

## Examples

### Example 1

```fsharp
AsyncResult.error "First"
|> AsyncResult.orElse (AsyncResult.ok "Second")
// evaluates to Ok "Second"
```

### Example 2

When the input is already `Ok`, the alternative is ignored:

```fsharp
AsyncResult.ok "First"
|> AsyncResult.orElse (AsyncResult.error "Second")
// evaluates to Ok "First"
```

---

## AsyncResult.orElseWith

Namespace: `FsToolkit.ErrorHandling`

Returns the original result unchanged if it is `Ok`, otherwise calls the given function with the error value and returns its result. The alternative function is **not** evaluated unless the input is `Error`.

## Function Signature:

```fsharp
('error -> Async<Result<'ok, 'error2>>)
  -> Async<Result<'ok, 'error>>
  -> Async<Result<'ok, 'error2>>
```

## Examples

### Example 1

```fsharp
AsyncResult.error "First"
|> AsyncResult.orElseWith (fun _ -> AsyncResult.error "Second")
// evaluates to Error "Second"
```

### Example 2

Falling back to a secondary data source when the primary fails:

```fsharp
let getUser (id: UserId) : Async<Result<User, string>> =
  fetchFromCache id
  |> AsyncResult.orElseWith (fun _ -> fetchFromDatabase id)
```

### Example 3

Using the error value to decide the fallback strategy:

```fsharp
let result =
  fetchData ()
  |> AsyncResult.orElseWith (fun err ->
      if isRetryable err then retryFetch ()
      else AsyncResult.error err)
```
