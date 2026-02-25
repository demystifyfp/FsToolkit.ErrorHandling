## CancellableTaskResult.singleton

Namespace: `FsToolkit.ErrorHandling`

Lifts a value into a `CancellableTaskResult`, wrapping it as an `Ok` result.

## Function Signature:

```fsharp
'item -> CancellableTaskResult<'item, 'Error>
```

## Examples

### Example 1

```fsharp
let result : CancellableTaskResult<int, string> =
  CancellableTaskResult.singleton 42
```

### Example 2

Using `singleton` inside a pipeline to return a successful value:

```fsharp
let getDefaultConfig () : CancellableTaskResult<Config, string> =
  CancellableTaskResult.singleton defaultConfig
```
