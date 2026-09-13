## JobOption.either

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Job<'b>) -> Job<'b> -> Job<'a option> -> Job<'b>
```

Runs `onSome` if the job-wrapped option is `Some`, otherwise runs `onNone`.

## Examples

### Example 1

```fsharp
let tryFindUser : string -> Job<User option>

// Job<string>
tryFindUser "alice"
|> JobOption.either
    (fun user -> Job.result $"Found user: {user.Name}"))
    (Job.result "User not found")
```

### Example 2

```fsharp
let maybeValue : Job<int option> = Job.result (Some 42)

maybeValue
|> JobOption.either
    (fun x -> Job.result (x * 2))
    (Job.result 0)
// job { return 84 }
```

### Example 3

```fsharp
let emptyJob : Job<int option> = Job.result None

emptyJob
|> JobOption.either
    (fun x -> Job.result (x * 2))
    (Job.result 0)
// job { return 0 }
```
