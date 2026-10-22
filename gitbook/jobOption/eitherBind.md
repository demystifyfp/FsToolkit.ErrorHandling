## JobOption.eitherBind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> (unit -> 'b) -> Job<'a option> -> Job<'b>
```

Runs `onSome` if the job-wrapped option is `Some`, otherwise runs `onNone`.

## Examples

### Example 1

```fsharp
let tryFindUser : string -> Job<User option>

// Job<string>
tryFindUser "alice"
|> JobOption.eitherBind
    (fun user -> $"Found user: %s{user.Name}")
    (fun () -> "User not found")
```

### Example 2

```fsharp
let maybeValue : Job<int option> = Job.result (Some 42)

maybeValue
|> JobOption.eitherBind
    (fun x -> x * 2)
    (fun () -> 0)
// job { return 84 }
```

### Example 3

```fsharp
let emptyJob : Job<int option> = Job.result None

emptyJob
|> JobOption.eitherBind
    (fun x -> x * 2)
    (fun () -> 0)
// job { return 0 }
```
