# Job.result

Namespace: `FsToolkit.ErrorHandling`

Lifts a value into a `Job`.

Function Signature:

```fsharp
'a -> Job<'a>
```

## Examples

### Example 1

```fsharp
Job.result 42
// job { return 42 }
```

### Example 2

```fsharp
Job.result "hello"
// job { return "hello" }
```

### Example 3

```fsharp
// Lift an existing value into the Job context for use with other Job functions
let value = { Name = "Alice"; Age = 30 }
Job.result value
// job { return { Name = "Alice"; Age = 30 } }
```
