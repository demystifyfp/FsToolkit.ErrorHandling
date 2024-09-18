## Seq.sequenceAsyncResultM

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
Async<Result<'a, 'b>> seq -> Async<Result<'a seq, 'b>>
```

Note that `sequence` is the same as `traverse id`. See also [Seq.traverseAsyncResultM](traverseAsyncResultM.md).

This is monadic, stopping on the first error.

This is the same as [sequenceResultM](sequenceResultM.md) except that it uses `Async<Result<_,_>>` instead of `Result<_,_>`.

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples
