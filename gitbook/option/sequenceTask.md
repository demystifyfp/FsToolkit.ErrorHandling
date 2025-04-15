## Option.sequenceTask

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Task<'a> option -> Task<'a option>
```

Note that `sequence` is the same as `traverse id`. See also [Option.traverseTask](traverseTask.md).

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

```fsharp
let a1 : Task<int option> =
  Option.sequenceTask (Some (Task.singleton 42))
// async { return Some 42 }

let a2 : Task<int option> =
  Option.sequenceTask None
// async { return None }
```
