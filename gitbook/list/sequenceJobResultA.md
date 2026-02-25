## List.sequenceJobResultA

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Job<Result<'a, 'b>> list -> Job<Result<'a list, 'b list>>
```

This is the same as [List.traverseJobResultA](traverseJobResultA.md) with `id` as the mapping function.

This is applicative, collecting all errors rather than stopping at the first. Compare with [sequenceJobResultM](sequenceJobResultM.md), which short-circuits on first error.

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

```fsharp
let jobs =
    [ JobResult.singleton 1
      JobResult.singleton 2
      JobResult.singleton 3 ]

jobs |> List.sequenceJobResultA
// job { return Ok [1; 2; 3] }
```

### Example 2

```fsharp
let jobs =
    [ JobResult.singleton 1
      JobResult.error "error1"
      JobResult.singleton 3
      JobResult.error "error2" ]

jobs |> List.sequenceJobResultA
// job { return Error ["error1"; "error2"] }
// collects all errors
```
