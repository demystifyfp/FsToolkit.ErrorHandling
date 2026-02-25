## List.traverseJobResultA

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Job<Result<'b, 'c>>) -> 'a list -> Job<Result<'b list, 'c list>>
```

Note that `traverse` is the same as `map >> sequence`. See also [List.sequenceJobResultA](sequenceJobResultA.md).

This is applicative, collecting all errors rather than stopping at the first. Compare with [traverseJobResultM](traverseJobResultM.md), which short-circuits on first error.

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

```fsharp
// string -> Job<Result<int, string>>
let tryParseIntJob str =
    job {
        match System.Int32.TryParse str with
        | true, x -> return Ok x
        | false, _ -> return Error (sprintf "unable to parse '%s' to integer" str)
    }

["1"; "2"; "3"]
|> List.traverseJobResultA tryParseIntJob
// job { return Ok [1; 2; 3] }

["1"; "foo"; "3"; "bar"]
|> List.traverseJobResultA tryParseIntJob
// job { return Error ["unable to parse 'foo' to integer"; "unable to parse 'bar' to integer"] }
// collects all errors
```

### Example 2

```fsharp
// int -> Job<Result<User, string>>
let fetchUser : UserId -> Job<Result<User, string>>

[userId1; userId2; userId3]
|> List.traverseJobResultA fetchUser
// job { return Ok [user1; user2; user3] }
// or Error with a list of all users that failed to fetch
```
