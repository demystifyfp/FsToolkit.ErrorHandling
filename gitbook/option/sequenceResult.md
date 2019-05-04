## Option.sequenceResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Result<'a,'b> option -> Result<'a option, 'b>
```

Note that `sequence` is the same as `traverse id`. See also [Option.traverseResult](traverseResult.md).

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

```fsharp
let r1 : Result<int option, string> =
  Option.sequenceResult (Some (Ok 42))
// Ok (Some 42)

let r2 : Result<int option, string> =
  Option.sequenceResult (Some (Error "something went wrong"))
// Error "something went wrong"

let r3 : Result<int option, string> =
  Option.sequenceResult None
// Ok None
```