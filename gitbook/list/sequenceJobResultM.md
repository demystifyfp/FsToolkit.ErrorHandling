## List.sequenceJobResultM

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
Job<Result<'a, 'b>> list -> Job<Result<'a list, 'b>>
```

This is the same as [List.traverseJobResultM](traverseJobResultM.md) with `id` as the mapping function.

This is monadic, stopping on the first error. Compare with [sequenceJobResultA](sequenceJobResultA.md), which collects all errors.

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

### Example 1

```fsharp
let jobs =
    [ JobResult.singleton 1
      JobResult.singleton 2
      JobResult.singleton 3 ]

jobs |> List.sequenceJobResultM
// job { return Ok [1; 2; 3] }
```

### Example 2

```fsharp
let jobs =
    [ JobResult.singleton 1
      JobResult.error "oops"
      JobResult.singleton 3 ]

jobs |> List.sequenceJobResultM
// job { return Error "oops" }
// stops at first error
```
