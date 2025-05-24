# CancellableTaskOption.zip

Namespace: `FsToolkit.ErrorHandling`

Takes two options and returns a tuple of the pair or None if either are None

## Function Signature

```fsharp
CancellableTask<'left option> -> CancellableTask<'right option> -> CancellableTask<('left * 'right) option>
```

## Examples

### Example 1

```fsharp
let left = CancellableTaskOption.some 123
let right = CancellableTaskOption.some "abc"

CancellableTaskOption.zip left right
// cancellableTask { Some (123, "abc") }
```

### Example 2

```fsharp
let left = CancellableTaskOption.some 123
let right = CancellableTaskOption.singleton None

CancellableTaskOption.zip left right
// cancellableTask { None }
```
