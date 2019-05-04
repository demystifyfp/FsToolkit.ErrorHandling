## AsyncResult.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Async<Result<'b, 'c>>)
  -> Async<Result<'a, 'c>>
  -> Async<Result<'b, 'c>>
```

## Examples

Note: Many use-cases requiring `bind` operations can also be solved using [the `asyncResult` computation expression](../asyncResult/ce.md).

### Example 1

Continuing from the [AsyncResult.map2 example](../asyncResult/map2.md#example-1) and given the function

```fsharp
let notifyFollowers : NotifyNewPostRequest -> Async<Result<unit,exn>>
```

we can notify all followers using `AsyncResult.bind` as below:

```fsharp
newPostRequestResult |> AsyncResult.bind notifyFollowers
```

