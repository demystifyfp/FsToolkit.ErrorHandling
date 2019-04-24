## List.traverseTaskResultA

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```
('a -> Task<Result<'b,'c>>) -> 'a list -> Task<Result<'b list, 'c list>>
```

Note that `traverse` is the same as `map >> sequence`. See also [List.sequenceTaskResultA](sequenceTaskResultA.md).

This is applicative, collecting all errors.

This is the same as [traverseResultA](traverseResultA.md) except that it uses `Task<Result<_,_>>` instead of `Result<_,_>`.

See also Scott Wlaschin's [Understanding traverse and sequence](https://fsharpforfunandprofit.com/posts/elevated-world-4/).

## Examples
