# TaskOption.map

Namespace: `FsToolkit.ErrorHandling`

Apply a function to the value of a task option if it is `Some`. If the option is `None`, return `None`.

## Function Signature

```fsharp
('input -> 'output) -> Task<'input option> -> Task<'output option>
```

## Examples

### Example 1

```fsharp
TaskOption.map (fun x -> x + 1) (TaskOption.some 1)

// task { Some 2 }
```

### Example 2

```fsharp
TaskOption.map (fun x -> x + 1) (Task.singleton None)

// task { None }
```

