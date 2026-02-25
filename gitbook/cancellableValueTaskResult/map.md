# CancellableValueTaskResult.map

Namespace: `FsToolkit.ErrorHandling`

Apply a function to the `Ok` value of a `CancellableValueTask<Result<'a, 'b>>` and return a new `CancellableValueTask<Result<'c, 'b>>`.

## Function Signature

```fsharp
('a -> 'b) -> CancellableValueTask<Result<'a, 'c>> -> CancellableValueTask<Result<'b, 'c>>
```

## Examples

### Example 1

```fsharp
CancellableValueTaskResult.map (fun x -> x + 1) (CancellableValueTaskResult.singleton 1)

// cancellableValueTask { Ok 2 }
```

### Example 2

```fsharp
let err : CancellableValueTask<Result<int, string>> = cancellableValueTask { return Error "some error" }
CancellableValueTaskResult.map (fun x -> x + 1) err

// cancellableValueTask { Error "some error" }
```
