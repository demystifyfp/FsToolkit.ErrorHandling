## Task.bindV

Namespace: `FsToolkit.ErrorHandling`

Like [Task.bind](../task/bind.md), but taking a `ValueTask<'a>` as input

Function Signature:

```fsharp
('a -> Task<'b>>) -> ValueTask<'a> -> Task<'b>
```

## Examples

Note: Many use-cases requiring `bind` operations can also be solved using [the `task` computation expression](../task/ce.md).

### Example 1

Continuing from the [Task.map2 example](../task/map2.md#example-1 and given the function

```fsharp
let notifyFollowers : NotifyNewPostRequest -> Task<unit>
```

and assuming `newPostRequestResult` has type `ValueTask<NotifyNewPostRequest>`

we can notify all followers using `Task.bindV` as below:

```fsharp
newPostRequestResult |> Task.bindV notifyFollowers
```

