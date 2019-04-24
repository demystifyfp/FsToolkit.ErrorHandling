## List.sequenceAsyncResultA

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
Async<Result<'a, 'b>> list -> Async<Result<'a list, 'b list>>
```

Note that `sequence` is the same as `traverse id`. See also [List.traverseAsyncResultA](traverseAsyncResultA.md).

This is applicative, collecting all errors.

This is the same as [sequenceResultA](sequenceResultA.md) except that it uses `Async<Result<_,_>>` instead of `Result<_,_>`.

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples

