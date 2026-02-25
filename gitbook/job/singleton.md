# Job.singleton

Namespace: `FsToolkit.ErrorHandling`

Lifts a value into a `Job`.

Function Signature:

```fsharp
'a -> Job<'a>
```

## Examples

### Example 1

```fsharp
Job.singleton 42
// job { return 42 }
```

### Example 2

```fsharp
Job.singleton "hello"
// job { return "hello" }
```

### Example 3

```fsharp
// Lift an existing value into the Job context for use with other Job functions
let value = { Name = "Alice"; Age = 30 }
Job.singleton value
// job { return { Name = "Alice"; Age = 30 } }
```
