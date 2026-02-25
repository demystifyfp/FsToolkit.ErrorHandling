## TaskResult.ok

Namespace: `FsToolkit.ErrorHandling`

Lift an `'ok` value into a `Task<Result<'ok, 'error>>`

## Function Signature:

```fsharp
'ok -> Task<Result<'ok, 'error>>
```

## Examples

### Example 1

```fsharp
let result : Task<Result<int, string>> =
  TaskResult.ok 42
```

### Example 2

Using `TaskResult.ok` inside a pipeline:

```fsharp
let defaultUser : Task<Result<User, string>> =
  TaskResult.ok { Id = UserId 0; Name = "Guest" }
```
