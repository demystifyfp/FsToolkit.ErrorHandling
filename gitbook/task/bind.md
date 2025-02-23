## Task.bind

Namespace: `FsToolkit.ErrorHandling`

Function Signature:

```fsharp
('a -> Task<'b>>) -> Task<'a> -> Task<'b>
```

## Examples

Note: Many use-cases requiring `bind` operations can also be solved using [the `task` computation expression](../task/ce.md).

### Example 1

Continuing from the [Task.map2 example](../task/map2.md#example-1) and given the function

```fsharp
let notifyFollowers : NotifyNewPostRequest -> Task<unit>
```

we can notify all followers using `Task.bind` as below:

```fsharp
newPostRequestResult |> Task.bind notifyFollowers
```

