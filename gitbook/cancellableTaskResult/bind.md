## CancellableTaskResult.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> CancellableTask<Result<'b, 'c>>)
  -> CancellableTask<Result<'a, 'c>>
  -> CancellableTask<Result<'b, 'c>>
```

## Examples

Note: Many use-cases requiring `bind` operations can also be solved using [the `cancellableTaskResult` computation expression](../cancellableTaskResult/ce.md).

### Example 1

Continuing from the [CancellableTaskResult.map2 example](../cancellableTaskResult/map2.md#example-1) and given the function

```fsharp
let notifyFollowers : NotifyNewPostRequest -> CancellableTask<Result<unit,exn>>
```

We can notify all followers using `CancellableTaskResult.bind` as below:

```fsharp
newPostRequestResult |> CancellableTaskResult.bind notifyFollowers
```

