## Seq.sequenceTaskResultA

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
Task<Result<'a, 'b>> seq -> Task<Result<'a seq, 'b seq>>
```

Note that `sequence` is the same as `traverse id`. See also [Seq.traverseTaskResultA](traverseTaskResultA.md).

This is applicative, collecting all errors.

This is the same as [sequenceResultA](sequenceResultA.md) except that it uses `Task<Result<_,_>>` instead of `Result<_,_>`.

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

