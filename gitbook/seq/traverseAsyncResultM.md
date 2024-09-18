## Seq.traverseAsyncResultM

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> Async<Result<'b,'c>>) -> 'a seq -> Async<Result<'b seq, 'c>>
```

Note that `traverse` is the same as `map >> sequence`. See also [Seq.sequenceAsyncResultM](sequenceAsyncResultM.md).

This is monadic, stopping on the first error.

This is the same as [traverseResultM](traverseResultM.md) except that it uses `Async<Result<_,_>>` instead of `Result<_,_>`.

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples
