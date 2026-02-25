## JobResultOption.map

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> 'b) -> Job<Result<'a option, 'c>> 
  -> Job<Result<'b option, 'c>>
```

## Examples

Note: Many use-cases requiring `map` operations can also be solved using [the `jobResultOption` computation expression](ce.md).

### Example 1

Given the function

```fsharp
getUserById : UserId -> Job<Result<User option, exn>>
```

Then using `JobResultOption.map` we can get the name of the user like this:

```fsharp
// Job<Result<PersonName option, exn>>
getUserById sampleUserId
|> JobResultOption.map (fun user -> user.Name)
```

### Example 2

```fsharp
// Job<Result<int option, string>>
let parseFirstItem = JobResult.singleton (Some "42")

parseFirstItem
|> JobResultOption.map int
// job { return Ok (Some 42) }
```
