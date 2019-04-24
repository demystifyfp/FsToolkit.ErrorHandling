## List.traverseAsyncResultA

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> Async<Result<'b,'c>>) -> 'a list -> Async<Result<'b list, 'c list>>
```

Note that `traverse` is the same as `map >> sequence`. See also [List.sequenceAsyncResultA](sequenceAsyncResultA.md).

This is applicative, collecting all errors.

This is the same as [traverseResultA](traverseResultA.md) except that it uses `Async<Result<_,_>>` instead of `Result<_,_>`.

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples
