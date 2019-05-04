## TaskResult.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Task<Result<'b, 'c>>)
  -> Task<Result<'a, 'c>>
  -> Task<Result<'b, 'c>>
```

## Examples

Note: Many use-cases requiring `bind` operations can also be solved using [the `taskResult` computation expression](../taskResult/ce.md).

### Example 1

Continuing from the [TaskResult.map2 example](../taskResult/map2.md#example-1) and given the function

```fsharp
let notifyFollowers : NotifyNewPostRequest -> Task<Result<unit,exn>>
```

we can notify all followers using `TaskResult.bind` as below:

```fsharp
newPostRequestResult |> TaskResult.bind notifyFollowers
```

