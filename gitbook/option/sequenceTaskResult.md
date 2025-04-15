## Option.sequenceTaskResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Task<Result<'a, 'e>> option -> Task<Result<'a option>, 'e>
```

Note that `sequence` is the same as `traverse id`. See also [Option.traverseTaskResult](traverseTaskResult.md).

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

```fsharp
let r1 : Task<Result<int option, string>> =
  Some (task { return Ok 42 }) |> Option.sequenceTaskResult 
// task { return Ok (Some 42) }

let r2 : Task<Result<int option, string>> =
  Some (task { return Error "something went wrong" }) |> Option.sequenceTaskResult 
// task { return Error "something went wrong" }

let r3 : Task<Result<int option, string>> =
  None |> Option.sequenceTaskResult 
// task { return Ok None }
```
