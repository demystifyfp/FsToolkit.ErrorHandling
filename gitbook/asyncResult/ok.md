## AsyncResult.ok

Namespace: `FsToolkit.ErrorHandling`

Lift an `'ok` value into an `Async<Result<'ok, 'error>>`

## Function Signature:

```fsharp
'ok -> Async<Result<'ok, 'error>>
```

## Examples

### Example 1

```fsharp
let result : Async<Result<int, string>> =
  AsyncResult.ok 42
```

### Example 2

Using `AsyncResult.ok` inside a pipeline:

```fsharp
let getUser (id: UserId) : Async<Result<User, string>> =
  asyncResult {
    let! user = fetchUser id
    return user
  }

// Lifting a simple value to AsyncResult:
let defaultUser : Async<Result<User, string>> =
  AsyncResult.ok { Id = UserId 0; Name = "Guest" }
```
