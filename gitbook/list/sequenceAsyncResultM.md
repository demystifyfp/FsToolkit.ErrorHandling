## List.sequenceAsyncResultM

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
Async<Result<'a, 'b>> list -> Async<Result<'a list, 'b>>
```

Note that `sequence` is the same as `traverse id`. See also [List.traverseAsyncResultM](traverseAsyncResultM.md).

This is monadic, stopping on the first error.

This is the same as [sequenceResultM](sequenceResultM.md) except that it uses `Async<Result<_,_>>` instead of `Result<_,_>`.

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples