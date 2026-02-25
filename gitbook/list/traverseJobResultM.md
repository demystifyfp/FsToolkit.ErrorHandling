## List.traverseJobResultM

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Job<Result<'b, 'c>>) -> 'a list -> Job<Result<'b list, 'c>>
```

Note that `traverse` is the same as `map >> sequence`. See also [List.sequenceJobResultM](sequenceJobResultM.md).

This is monadic, stopping on the first error. Compare with [traverseJobResultA](traverseJobResultA.md), which collects all errors.

This is the same as [traverseResultM](traverseResultM.md) except that it uses `Job<Result<_,_>>` instead of `Result<_,_>`.

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
|> List.traverseJobResultM tryParseIntJob
// job { return Ok [1; 2; 3] }

["1"; "foo"; "3"; "bar"]
|> List.traverseJobResultM tryParseIntJob
// job { return Error "unable to parse 'foo' to integer" }
// stops at first error
```

### Example 2

```fsharp
// int -> Job<Result<User, string>>
let fetchUser : UserId -> Job<Result<User, string>>

[userId1; userId2; userId3]
|> List.traverseJobResultM fetchUser
// job { return Ok [user1; user2; user3] }
// or Error at the first user that fails to fetch
```
