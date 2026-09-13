## CancellableTaskValidation.ok

Namespace: `FsToolkit.ErrorHandling`

Lifts a value into a `CancellableTaskValidation`, wrapping it as an `Ok` result. This is the primary way to create a successful `CancellableTaskValidation` value.

## Function Signature:

```fsharp
'item -> CancellableTaskValidation<'item, 'Error>
```

## Examples

### Example 1

```fsharp
let result : CancellableTaskValidation<int, string> =
  CancellableTaskValidation.ok 42
```

### Example 2

Using `ok` inside a pipeline to return a validated value:

```fsharp
let validateAge (age: int) : CancellableTaskValidation<int, string> =
  if age >= 0 then CancellableTaskValidation.ok age
  else CancellableTaskValidation.error "Age must be non-negative"
```
