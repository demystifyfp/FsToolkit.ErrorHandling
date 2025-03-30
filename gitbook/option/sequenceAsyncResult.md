## Option.sequenceAsyncResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Async<Result<'a, 'e>> option -> Async<Result<'a option>, 'e>
```

Note that `sequence` is the same as `traverse id`. See also [Option.traverseAsyncResult](traverseAsyncResult.md).

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

```fsharp
let r1 : Async<Result<int option, string>> =
  Some (async { return Ok 42 }) |> Option.sequenceAsyncResult 
// async { return Ok (Some 42) }

let r2 : Async<Result<int option, string>> =
  Some (async { return Error "something went wrong" }) |> Option.sequenceAsyncResult 
// async { return Error "something went wrong" }

let r3 : Async<Result<int option, string>> =
  None |> Option.sequenceAsyncResult 
// async { return Ok None }
```
