# CancellableValueTaskResult.zip

Namespace: `FsToolkit.ErrorHandling`

Takes two results and returns a tuple of the pair, or the first error encountered.

## Function Signature

```fsharp
CancellableValueTask<Result<'left, 'error>> -> CancellableValueTask<Result<'right, 'error>> -> CancellableValueTask<Result<('left * 'right), 'error>>
```

## Examples

### Example 1

```fsharp
let left = CancellableValueTaskResult.singleton 123
let right = CancellableValueTaskResult.singleton "abc"

CancellableValueTaskResult.zip left right
// cancellableValueTask { Ok (123, "abc") }
```

### Example 2

```fsharp
let left : CancellableValueTask<Result<int, string>> = cancellableValueTask { return Error "left error" }
let right = CancellableValueTaskResult.singleton "abc"

CancellableValueTaskResult.zip left right
// cancellableValueTask { Error "left error" }
```
