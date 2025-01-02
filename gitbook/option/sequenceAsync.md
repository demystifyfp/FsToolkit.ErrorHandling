## Option.sequenceAsync

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Async<'a> option -> Async<'a option>
```

Note that `sequence` is the same as `traverse id`. See also [Option.traverseAsync](traverseAsync.md).

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

```fsharp
let a1 : Async<int option> =
  sequenceResult (Some (Async.singleton 42))
// async { return Some 42 }

let a2 : Async<int option> =
  sequenceAsync None
// async { return None }
```
