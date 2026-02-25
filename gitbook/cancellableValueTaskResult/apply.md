# CancellableValueTaskResult.apply

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
CancellableValueTask<Result<('a -> 'b), 'c>> -> CancellableValueTask<Result<'a, 'c>> -> CancellableValueTask<Result<'b, 'c>>
```

## Examples

### Example 1

```fsharp
let result =
    CancellableValueTaskResult.singleton "foo"
    |> CancellableValueTaskResult.apply (CancellableValueTaskResult.singleton String.length)

// cancellableValueTask { Ok 3 }
```

### Example 2

```fsharp
let err : CancellableValueTask<Result<int, string>> = cancellableValueTask { return Error "some error" }
let result =
    err
    |> CancellableValueTaskResult.apply (CancellableValueTaskResult.singleton String.length)

// cancellableValueTask { Error "some error" }
```
