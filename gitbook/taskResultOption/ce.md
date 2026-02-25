## TaskResultOption Computation Expression

Namespace: `FsToolkit.ErrorHandling`

## Examples 

### Example 1

The [TaskResultOption.map2 example](../taskResultOption/map2.md#example-1) can be written using the `taskResultOption` computation expression as below:

```fsharp
// Task<Result<UserTweet option, exn>>
taskResultOption {
  let! post = getPostById samplePostId
  let! user = getUserById post.UserId
  return userTweet post user
}
```

### Example 2

The `taskResultOption` CE supports binding from multiple compatible types, allowing you to mix `TaskResultOption`, `TaskResult`, `Result`, `Option`, `Task`, and `Async` values without manual conversion:

```fsharp
// parseMode: string -> Result<Mode, string>
// loadUniverse: Mode -> Task<Result<Universe, string>>

// Task<Result<Report option, string>>
taskResultOption {
  // Bind from Result<Mode, string> directly (no manual conversion needed)
  let! mode = parseMode "fast"

  // Bind from Task<Result<Universe, string>> directly
  let! universe = loadUniverse mode

  return generateReport universe
}
```

### Supported Source Types

The `taskResultOption` CE supports `let!` and `return!` with the following types:

| Type | Behavior |
|------|----------|
| `Task<Result<'ok option, 'error>>` | Identity (the CE's native type) |
| `Task<Result<'ok, 'error>>` | `Ok` value is wrapped in `Some` |
| `Task<'ok option>` | `Some`/`None` is wrapped in `Ok` |
| `Task<'ok>` | Value is wrapped in `Some` and then `Ok` |
| `Result<'ok, 'error>` | `Ok` value is wrapped in `Some`, lifted to `Task` |
| `Choice<'ok, 'error>` | Converted to `Result`, then same as `Result` |
| `'ok option` | `Some`/`None` wrapped in `Ok`, lifted to `Task` |
| `Async<Result<'ok, 'error>>` | `Ok` value is wrapped in `Some` |
| `Async<'ok>` | Value is wrapped in `Some` and then `Ok` |
