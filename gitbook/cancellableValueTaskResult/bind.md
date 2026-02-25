# CancellableValueTaskResult.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> CancellableValueTask<Result<'b, 'c>>)
  -> CancellableValueTask<Result<'a, 'c>>
  -> CancellableValueTask<Result<'b, 'c>>
```

## Examples

Note: Many use-cases requiring `bind` operations can also be solved using [the `cancellableValueTaskResult` computation expression](../cancellableValueTaskResult/ce.md).

### Example 1

Continuing from the CancellableValueTaskResult.map2 example and given the function

```fsharp
let notifyFollowers : NotifyNewPostRequest -> CancellableValueTask<Result<unit,exn>>
```

We can notify all followers using `CancellableValueTaskResult.bind` as below:

```fsharp
newPostRequestResult |> CancellableValueTaskResult.bind notifyFollowers
```
