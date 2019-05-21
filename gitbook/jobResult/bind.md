## JobResult.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Job<Result<'b, 'c>>)
  -> Job<Result<'a, 'c>>
  -> Job<Result<'b, 'c>>
```

## Examples

Note: Many use-cases requiring `bind` operations can also be solved using [the `jobResult` computation expression](../jobResult/ce.md).

### Example 1

Continuing from the [JobResult.map2 example](../jobResult/map2.md#example-1) and given the function

```fsharp
let notifyFollowers : NotifyNewPostRequest -> Job<Result<unit,exn>>
```

we can notify all followers using `JobResult.bind` as below:

```fsharp
newPostRequestResult |> JobResult.bind notifyFollowers
```

