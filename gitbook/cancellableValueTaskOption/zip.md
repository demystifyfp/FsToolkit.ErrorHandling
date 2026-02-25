# CancellableValueTaskOption.zip

Namespace: `FsToolkit.ErrorHandling`

Takes two options and returns a tuple of the pair or None if either are None

## Function Signature

```fsharp
CancellableValueTask<'left option> -> CancellableValueTask<'right option> -> CancellableValueTask<('left * 'right) option>
```

## Examples

### Example 1

```fsharp
let left = CancellableValueTaskOption.some 123
let right = CancellableValueTaskOption.some "abc"

CancellableValueTaskOption.zip left right
// cancellableValueTask { Some (123, "abc") }
```

### Example 2

```fsharp
let left = CancellableValueTaskOption.some 123
let right = CancellableValueTask.singleton None

CancellableValueTaskOption.zip left right
// cancellableValueTask { None }
```
