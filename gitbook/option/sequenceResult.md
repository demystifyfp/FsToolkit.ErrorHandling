## Option.sequenceResult

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```F#
Result<'a,'b> option -> Result<'a option, 'b>
```

Note that `Option.sequenceResult` is the same as `Option.traverseResult id`.

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