## TaskResult.orElse

Namespace: `FsToolkit.ErrorHandling`

Returns the original result unchanged if it is `Ok`, otherwise returns the provided alternative result.

## Function Signature:

```fsharp
Task<Result<'ok, 'error2>>
  -> Task<Result<'ok, 'error>>
  -> Task<Result<'ok, 'error2>>
```

## Examples

### Example 1

```fsharp
TaskResult.error "First"
|> TaskResult.orElse (TaskResult.ok "Second")
// evaluates to Ok "Second"
```

### Example 2

When the input is already `Ok`, the alternative is ignored:

```fsharp
TaskResult.ok "First"
|> TaskResult.orElse (TaskResult.error "Second")
// evaluates to Ok "First"
```

---

## TaskResult.orElseWith

Namespace: `FsToolkit.ErrorHandling`

Returns the original result unchanged if it is `Ok`, otherwise calls the given function with the error value and returns its result. The alternative function is **not** evaluated unless the input is `Error`.

## Function Signature:

```fsharp
('error -> Task<Result<'ok, 'error2>>)
  -> Task<Result<'ok, 'error>>
  -> Task<Result<'ok, 'error2>>
```

## Examples

### Example 1

```fsharp
TaskResult.error "First"
|> TaskResult.orElseWith (fun _ -> TaskResult.error "Second")
// evaluates to Error "Second"
```

### Example 2

Falling back to a secondary data source when the primary fails:

```fsharp
let getUser (id: UserId) : Task<Result<User, string>> =
  fetchFromCache id
  |> TaskResult.orElseWith (fun _ -> fetchFromDatabase id)
```

### Example 3

Using the error value to decide the fallback strategy:

```fsharp
let result =
  fetchData ()
  |> TaskResult.orElseWith (fun err ->
      if isRetryable err then retryFetch ()
      else TaskResult.error err)
```
